namespace CodeProject.ObjectPool.Specialized
{
	public interface IMemoryStreamPool : IObjectPool<PooledMemoryStream>
	{
		int MinimumMemoryStreamCapacity
		{
			get;
			set;
		}

		int MaximumMemoryStreamCapacity
		{
			get;
			set;
		}
	}
}
