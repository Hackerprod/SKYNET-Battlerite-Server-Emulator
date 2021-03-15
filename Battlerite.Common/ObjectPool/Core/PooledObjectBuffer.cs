using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CodeProject.ObjectPool.Core
{
	public sealed class PooledObjectBuffer<T> : IEnumerable<T>, IEnumerable where T : PooledObject
	{
		private const MethodImplOptions TryInline = MethodImplOptions.AggressiveInlining;

		private static readonly T[] NoObjects = new T[0];

		private T[] _pooledObjects = NoObjects;

		public int Capacity => _pooledObjects.Length;

		public int Count
		{
			get
			{
				int num = 0;
				for (int i = 0; i < _pooledObjects.Length; i++)
				{
					if ((PooledObject)_pooledObjects[i] != (PooledObject)null)
					{
						num++;
					}
				}
				return num;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			T[] pooledObjects = _pooledObjects;
			foreach (T val in pooledObjects)
			{
				if ((PooledObject)val != (PooledObject)null)
				{
					yield return val;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryDequeue(out T pooledObject)
		{
			for (int i = 0; i < _pooledObjects.Length; i++)
			{
				T val = _pooledObjects[i];
				if ((PooledObject)val != (PooledObject)null && (PooledObject)Interlocked.CompareExchange(ref _pooledObjects[i], null, val) == (PooledObject)val)
				{
					pooledObject = val;
					return true;
				}
			}
			pooledObject = null;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryEnqueue(T pooledObject)
		{
			for (int i = 0; i < _pooledObjects.Length; i++)
			{
				ref T reference = ref _pooledObjects[i];
				if ((PooledObject)reference == (PooledObject)null && (PooledObject)Interlocked.CompareExchange(ref reference, pooledObject, null) == (PooledObject)null)
				{
					return true;
				}
			}
			return false;
		}

		public IList<T> Resize(int newCapacity)
		{
			if (_pooledObjects == NoObjects)
			{
				_pooledObjects = new T[newCapacity];
				return NoObjects;
			}
			int num = _pooledObjects.Length;
			if (num == newCapacity)
			{
				return NoObjects;
			}
			IList<T> list = NoObjects;
			if (num > newCapacity)
			{
				for (int i = newCapacity; i < num; i++)
				{
					ref T reference = ref _pooledObjects[i];
					if ((PooledObject)reference != (PooledObject)null)
					{
						if (list == NoObjects)
						{
							list = new List<T>
							{
								reference
							};
						}
						else
						{
							list.Add(reference);
						}
						reference = null;
					}
				}
			}
			Array.Resize(ref _pooledObjects, newCapacity);
			return list;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryRemove(T pooledObject)
		{
			for (int i = 0; i < _pooledObjects.Length; i++)
			{
				T val = _pooledObjects[i];
				if ((PooledObject)val != (PooledObject)null && (PooledObject)val == (PooledObject)pooledObject && (PooledObject)Interlocked.CompareExchange(ref _pooledObjects[i], null, val) == (PooledObject)val)
				{
					return true;
				}
			}
			return false;
		}
	}
}
