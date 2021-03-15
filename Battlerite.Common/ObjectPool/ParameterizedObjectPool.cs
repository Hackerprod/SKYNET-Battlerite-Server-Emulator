using CodeProject.ObjectPool.Core;
using System;
using System.Collections;
using System.Linq;

namespace CodeProject.ObjectPool
{
	public class ParameterizedObjectPool<TKey, TValue> : IParameterizedObjectPool<TKey, TValue> where TValue : PooledObject
	{
		private ObjectPoolDiagnostics _diagnostics;

		private int _maximumPoolSize;

		private readonly Hashtable _pools = new Hashtable();

		public ObjectPoolDiagnostics Diagnostics
		{
			get
			{
				return _diagnostics;
			}
			set
			{
				ObjectPool<TValue>[] array = _pools.Values.Cast<ObjectPool<TValue>>().ToArray();
				_diagnostics = value;
				ObjectPool<TValue>[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].Diagnostics = value;
				}
			}
		}

		public int MaximumPoolSize
		{
			get
			{
				return _maximumPoolSize;
			}
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException("value", "Maximum pool size must be greater than zero.");
				}
				_maximumPoolSize = value;
			}
		}

		public Func<TKey, TValue> FactoryMethod
		{
			get;
			private set;
		}

		public int KeysInPoolCount => _pools.Count;

		public ParameterizedObjectPool()
			: this(16, (Func<TKey, TValue>)null)
		{
		}

		public ParameterizedObjectPool(int maximumPoolSize)
			: this(maximumPoolSize, (Func<TKey, TValue>)null)
		{
		}

		public ParameterizedObjectPool(Func<TKey, TValue> factoryMethod)
			: this(16, factoryMethod)
		{
		}

		public ParameterizedObjectPool(int maximumPoolSize, Func<TKey, TValue> factoryMethod)
		{
			if (maximumPoolSize < 1)
			{
				throw new ArgumentOutOfRangeException("maximumPoolSize", "Maximum pool size must be greater than zero.");
			}
			Diagnostics = new ObjectPoolDiagnostics();
			FactoryMethod = factoryMethod;
			_maximumPoolSize = maximumPoolSize;
		}

		public void Clear()
		{
			ClearPools();
		}

		public TValue GetObject(TKey key)
		{
			if (!TryGetPool(key, out ObjectPool<TValue> objectPool))
			{
				objectPool = AddPool(key);
			}
			return objectPool.GetObject();
		}

		private void ClearPools()
		{
			ObjectPool<TValue>[] array = _pools.Values.Cast<ObjectPool<TValue>>().ToArray();
			lock (_pools)
			{
				_pools.Clear();
			}
			ObjectPool<TValue>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Clear();
			}
		}

		private bool TryGetPool(TKey key, out ObjectPool<TValue> objectPool)
		{
			objectPool = (_pools[key] as ObjectPool<TValue>);
			return objectPool != null;
		}

		private ObjectPool<TValue> AddPool(TKey key)
		{
			lock (_pools)
			{
				ObjectPool<TValue> result;
				if (!_pools.ContainsKey(key))
				{
					Hashtable pools = _pools;
					object key2 = key;
					ObjectPool<TValue> obj = new ObjectPool<TValue>(MaximumPoolSize, PrepareFactoryMethod(key))
					{
						Diagnostics = _diagnostics
					};
					result = obj;
					pools.Add(key2, obj);
				}
				else
				{
					result = (_pools[key] as ObjectPool<TValue>);
				}
				return result;
			}
		}

		private Func<TValue> PrepareFactoryMethod(TKey key)
		{
			Func<TKey, TValue> factory = FactoryMethod;
			if (factory == null)
			{
				return null;
			}
			return () => factory(key);
		}
	}
}
