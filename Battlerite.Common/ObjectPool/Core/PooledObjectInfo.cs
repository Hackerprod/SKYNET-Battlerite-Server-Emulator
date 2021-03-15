using System;

namespace CodeProject.ObjectPool.Core
{
	public sealed class PooledObjectInfo : IEquatable<PooledObjectInfo>
	{
		public int Id
		{
			get;
			internal set;
		}

		public object Payload
		{
			get;
			set;
		}

		public PooledObjectState State
		{
			get;
			internal set;
		}

		internal IObjectPoolHandle Handle
		{
			get;
			set;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}", "Id", Id, "Payload", Payload);
		}

		public bool Equals(PooledObjectInfo other)
		{
			if ((object)other == null)
			{
				return false;
			}
			if ((object)this == other)
			{
				return true;
			}
			return Id == other.Id;
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
			PooledObjectInfo pooledObjectInfo = obj as PooledObjectInfo;
			if (pooledObjectInfo != null)
			{
				return Equals(pooledObjectInfo);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Id;
		}

		public static bool operator ==(PooledObjectInfo left, PooledObjectInfo right)
		{
			return object.Equals(left, right);
		}

		public static bool operator !=(PooledObjectInfo left, PooledObjectInfo right)
		{
			return !object.Equals(left, right);
		}
	}
}
