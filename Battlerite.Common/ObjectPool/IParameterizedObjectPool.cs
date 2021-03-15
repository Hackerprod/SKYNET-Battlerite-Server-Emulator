using CodeProject.ObjectPool.Core;
using System;

namespace CodeProject.ObjectPool
{
	public interface IParameterizedObjectPool<in TKey, out TValue>
	{
		ObjectPoolDiagnostics Diagnostics
		{
			get;
			set;
		}

		Func<TKey, TValue> FactoryMethod
		{
			get;
		}

		int MaximumPoolSize
		{
			get;
			set;
		}

		int KeysInPoolCount
		{
			get;
		}

		TValue GetObject(TKey key);
	}
}
