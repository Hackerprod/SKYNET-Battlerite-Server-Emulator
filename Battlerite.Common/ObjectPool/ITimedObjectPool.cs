using System;

namespace CodeProject.ObjectPool
{
	public interface ITimedObjectPool<out T> : IObjectPool<T> where T : PooledObject
	{
		TimeSpan Timeout
		{
			get;
			set;
		}
	}
}
