namespace CodeProject.ObjectPool.Core
{
	internal interface IObjectPoolHandle
	{
		void ReturnObjectToPool(PooledObject objectToReturnToPool, bool reRegisterForFinalization);
	}
}
