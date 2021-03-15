namespace CodeProject.ObjectPool.Specialized
{
	public interface IStringBuilderPool : IObjectPool<PooledStringBuilder>
	{
		int MinimumStringBuilderCapacity
		{
			get;
			set;
		}

		int MaximumStringBuilderCapacity
		{
			get;
			set;
		}

		PooledStringBuilder GetObject(string value);
	}
}
