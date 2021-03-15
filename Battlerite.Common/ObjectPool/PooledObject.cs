using CodeProject.ObjectPool.Core;
using CodeProject.ObjectPool.Logging;
using System;

namespace CodeProject.ObjectPool
{
	[Serializable]
	public abstract class PooledObject : IDisposable, IEquatable<PooledObject>
	{
        public PooledObjectInfo PooledObjectInfo
		{
			get;
		} = new PooledObjectInfo();


		public Func<PooledObjectValidationContext, bool> OnValidateObject
		{
			get;
			set;
		}

		public Action OnResetState
		{
			get;
			set;
		}

		public Action OnReleaseResources
		{
			get;
			set;
		}

		internal bool ValidateObject(PooledObjectValidationContext validationContext)
		{
			if (OnValidateObject != null)
			{
				try
				{
					return OnValidateObject(validationContext);
				}
				catch (Exception exception)
				{
                    return false;
				}
			}
			return true;
		}

		internal bool ReleaseResources()
		{
			if (OnReleaseResources != null)
			{
				try
				{
					OnReleaseResources();
				}
				catch (Exception exception)
				{
					return false;
				}
			}
			return true;
		}

		internal bool ResetState()
		{
			if (!ValidateObject(PooledObjectValidationContext.Inbound(this)))
			{
				return false;
			}
			if (OnResetState != null)
			{
				try
				{
					OnResetState();
				}
				catch (Exception exception)
				{
					return false;
				}
			}
			return true;
		}

		public void Dispose()
		{
			HandleReAddingToPool(reRegisterForFinalization: false);
		}

		private void HandleReAddingToPool(bool reRegisterForFinalization)
		{
			if (PooledObjectInfo.State != PooledObjectState.Disposed && PooledObjectInfo.State != 0)
			{
				try
				{
					PooledObjectInfo.Handle.ReturnObjectToPool(this, reRegisterForFinalization);
				}
				catch (Exception exception)
				{
					PooledObjectInfo.State = PooledObjectState.Disposed;
					ReleaseResources();
				}
			}
		}

		~PooledObject()
		{
			HandleReAddingToPool(reRegisterForFinalization: true);
		}

		public override string ToString()
		{
			return PooledObjectInfo.ToString();
		}

		public virtual bool Equals(PooledObject other)
		{
			if ((object)other == null)
			{
				return false;
			}
			if ((object)this == other)
			{
				return true;
			}
			return PooledObjectInfo.Equals(other.PooledObjectInfo);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (this == obj)
			{
				return true;
			}
			if (obj.GetType() == GetType())
			{
				return Equals(obj as PooledObject);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return PooledObjectInfo.GetHashCode();
		}

		public static bool operator ==(PooledObject left, PooledObject right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(PooledObject left, PooledObject right)
		{
			return !object.Equals(left, right);
		}
	}
}
