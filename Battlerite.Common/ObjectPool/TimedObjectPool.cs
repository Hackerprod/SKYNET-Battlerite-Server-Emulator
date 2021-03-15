using CodeProject.ObjectPool.Core;
using System;

namespace CodeProject.ObjectPool
{
	public class TimedObjectPool<T> : ObjectPool<T>, ITimedObjectPool<T>, IObjectPool<T> where T : PooledObject
	{
		private TimeSpan _timeout;

		public TimeSpan Timeout
		{
			get
			{
				return _timeout;
			}
			set
			{
				StartEvictor(new EvictionSettings
				{
					Enabled = true,
					Delay = value,
					Period = value
				});
				_timeout = value;
			}
		}

		public TimedObjectPool(TimeSpan timeout)
			: this(16, (Func<T>)null, timeout)
		{
		}

		public TimedObjectPool(int maximumPoolSize, TimeSpan timeout)
			: this(maximumPoolSize, (Func<T>)null, timeout)
		{
		}

		public TimedObjectPool(Func<T> factoryMethod, TimeSpan timeout)
			: this(16, factoryMethod, timeout)
		{
		}

		public TimedObjectPool(int maximumPoolSize, Func<T> factoryMethod, TimeSpan timeout)
			: base(maximumPoolSize, factoryMethod, new EvictionSettings
			{
				Enabled = true,
				Delay = timeout,
				Period = timeout
			}, (IEvictionTimer)null)
		{
			if (timeout <= TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("timeout", "Timeout must be greater than zero.");
			}
			_timeout = timeout;
		}

		protected override T CreatePooledObject()
		{
			T pooledObject = base.CreatePooledObject();
			ref T reference = ref pooledObject;
			reference.OnResetState = (Action)Delegate.Combine(reference.OnResetState, (Action)delegate
			{
				pooledObject.PooledObjectInfo.Payload = DateTime.UtcNow;
			});
			reference = ref pooledObject;
			reference.OnValidateObject = (Func<PooledObjectValidationContext, bool>)Delegate.Combine(reference.OnValidateObject, (Func<PooledObjectValidationContext, bool>)delegate(PooledObjectValidationContext ctx)
			{
				DateTime t = DateTime.UtcNow - _timeout;
				object payload = ctx.PooledObjectInfo.Payload;
				bool num = payload is DateTime;
				DateTime t2 = num ? ((DateTime)payload) : default(DateTime);
				if (num)
				{
					return !(t2 < t);
				}
				return true;
			});
			return pooledObject;
		}
	}
}
