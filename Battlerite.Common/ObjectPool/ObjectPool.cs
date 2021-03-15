using CodeProject.ObjectPool.Core;
using System;
using System.Linq;
using System.Threading;

namespace CodeProject.ObjectPool
{
	public static class ObjectPool
	{
		public const int DefaultPoolMaximumSize = 16;
	}
	public class ObjectPool<T> : IObjectPool<T>, IObjectPoolHandle where T : PooledObject
	{
		private readonly IEvictionTimer _evictionTimer;

		private int _lastPooledObjectId;

		private Guid _evictionActionTicket;

		public ObjectPoolDiagnostics Diagnostics
		{
			get;
			set;
		}

		public Func<T> FactoryMethod
		{
			get;
			protected set;
		}

		public int MaximumPoolSize
		{
			get
			{
				return PooledObjects.Capacity;
			}
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("value", "Maximum pool size must be greater than zero.");
				}
				foreach (T item in PooledObjects.Resize(value))
				{
					DestroyPooledObject(item);
				}
			}
		}

		public int ObjectsInPoolCount => PooledObjects.Count;

		protected PooledObjectBuffer<T> PooledObjects
		{
			get;
		} = new PooledObjectBuffer<T>();


		public ObjectPool()
			: this(16, (Func<T>)null, (EvictionSettings)null, (IEvictionTimer)null)
		{
		}

		public ObjectPool(int maximumPoolSize)
			: this(maximumPoolSize, (Func<T>)null, (EvictionSettings)null, (IEvictionTimer)null)
		{
		}

		public ObjectPool(Func<T> factoryMethod)
			: this(16, factoryMethod, (EvictionSettings)null, (IEvictionTimer)null)
		{
		}

		public ObjectPool(int maximumPoolSize, Func<T> factoryMethod)
			: this(maximumPoolSize, factoryMethod, (EvictionSettings)null, (IEvictionTimer)null)
		{
		}

		public ObjectPool(EvictionSettings evictionSettings)
			: this(16, (Func<T>)null, evictionSettings, (IEvictionTimer)null)
		{
		}

		public ObjectPool(int maximumPoolSize, Func<T> factoryMethod, EvictionSettings evictionSettings, IEvictionTimer evictionTimer)
		{
			if (maximumPoolSize <= 0)
			{
				throw new ArgumentOutOfRangeException("maximumPoolSize", "Maximum pool size must be greater than zero.");
			}
			FactoryMethod = (factoryMethod ?? new Func<T>(Activator.CreateInstance<T>));
			MaximumPoolSize = maximumPoolSize;
			Diagnostics = new ObjectPoolDiagnostics();
			_evictionTimer = (evictionTimer ?? new EvictionTimer());
			StartEvictor(evictionSettings ?? EvictionSettings.Default);
		}

		~ObjectPool()
		{
			Clear();
			_evictionTimer?.Dispose();
		}

		public void Clear()
		{
			T pooledObject;
			while (PooledObjects.TryDequeue(out pooledObject))
			{
				DestroyPooledObject(pooledObject);
			}
		}

		public T GetObject()
		{
			T pooledObject;
			while (true)
			{
				if (PooledObjects.TryDequeue(out pooledObject))
				{
					if (Diagnostics.Enabled)
					{
						Diagnostics.IncrementPoolObjectHitCount();
					}
				}
				else
				{
					if (Diagnostics.Enabled)
					{
						Diagnostics.IncrementPoolObjectMissCount();
					}
					pooledObject = CreatePooledObject();
				}
				if (pooledObject.ValidateObject(PooledObjectValidationContext.Outbound(pooledObject)))
				{
					break;
				}
				DestroyPooledObject(pooledObject);
			}
			pooledObject.PooledObjectInfo.State = PooledObjectState.Reserved;
			return pooledObject;
		}

		void IObjectPoolHandle.ReturnObjectToPool(PooledObject objectToReturnToPool, bool reRegisterForFinalization)
		{
			T val = objectToReturnToPool as T;
			if (reRegisterForFinalization && Diagnostics.Enabled)
			{
				Diagnostics.IncrementObjectResurrectionCount();
			}
			if ((PooledObject)val != (PooledObject)null && !val.ResetState())
			{
				if (Diagnostics.Enabled)
				{
					Diagnostics.IncrementResetStateFailedCount();
				}
				DestroyPooledObject(val);
			}
			else
			{
				if (reRegisterForFinalization)
				{
					GC.ReRegisterForFinalize(val);
				}
				if (PooledObjects.TryEnqueue(val))
				{
					if (Diagnostics.Enabled)
					{
						Diagnostics.IncrementReturnedToPoolCount();
					}
					val.PooledObjectInfo.State = PooledObjectState.Available;
				}
				else
				{
					if (Diagnostics.Enabled)
					{
						Diagnostics.IncrementPoolOverflowCount();
					}
					DestroyPooledObject(val);
				}
			}
		}

		protected virtual T CreatePooledObject()
		{
			if (Diagnostics.Enabled)
			{
				Diagnostics.IncrementObjectsCreatedCount();
			}
			if (FactoryMethod != null)
			{
				T val = FactoryMethod();
				val.PooledObjectInfo.Id = Interlocked.Increment(ref _lastPooledObjectId);
				val.PooledObjectInfo.State = PooledObjectState.Available;
				val.PooledObjectInfo.Handle = this;
				return val;
			}
			return null;
		}

		protected void DestroyPooledObject(PooledObject objectToDestroy)
		{
			if (objectToDestroy.PooledObjectInfo.State != PooledObjectState.Disposed)
			{
				if (Diagnostics.Enabled)
				{
					Diagnostics.IncrementObjectsDestroyedCount();
				}
				objectToDestroy.ReleaseResources();
				objectToDestroy.PooledObjectInfo.State = PooledObjectState.Disposed;
			}
			GC.SuppressFinalize(objectToDestroy);
		}

		protected void StartEvictor(EvictionSettings settings)
		{
			if (settings.Enabled)
			{
				lock (this)
				{
					if (_evictionActionTicket != Guid.Empty)
					{
						_evictionTimer.Cancel(_evictionActionTicket);
					}
					_evictionActionTicket = _evictionTimer.Schedule(delegate
					{
						T[] array = PooledObjects.ToArray();
						foreach (T val in array)
						{
							if (!val.ValidateObject(PooledObjectValidationContext.Outbound(val)) && PooledObjects.TryRemove(val))
							{
								DestroyPooledObject(val);
							}
						}
					}, settings.Delay, settings.Period);
				}
			}
		}
	}
}
