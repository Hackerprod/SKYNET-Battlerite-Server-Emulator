using CodeProject.ObjectPool.Core;
using System;

namespace CodeProject.ObjectPool
{
	public interface IObjectPool<out T> where T : PooledObject
	{
		ObjectPoolDiagnostics Diagnostics
		{
			get;
			set;
		}

		Func<T> FactoryMethod
		{
			get;
		}

		int MaximumPoolSize
		{
			get;
			set;
		}

		int ObjectsInPoolCount
		{
			get;
		}

		void Clear();

		T GetObject();
	}
}
