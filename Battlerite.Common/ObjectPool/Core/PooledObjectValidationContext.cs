namespace CodeProject.ObjectPool.Core
{
	public sealed class PooledObjectValidationContext
	{
		public PooledObject PooledObject
		{
			get;
			private set;
		}

		public PooledObjectInfo PooledObjectInfo => PooledObject.PooledObjectInfo;

		public PooledObjectDirection Direction
		{
			get;
			private set;
		}

		internal static PooledObjectValidationContext Inbound(PooledObject pooledObject)
		{
			return new PooledObjectValidationContext
			{
				PooledObject = pooledObject,
				Direction = PooledObjectDirection.Inbound
			};
		}

		internal static PooledObjectValidationContext Outbound(PooledObject pooledObject)
		{
			return new PooledObjectValidationContext
			{
				PooledObject = pooledObject,
				Direction = PooledObjectDirection.Outbound
			};
		}
	}
}
