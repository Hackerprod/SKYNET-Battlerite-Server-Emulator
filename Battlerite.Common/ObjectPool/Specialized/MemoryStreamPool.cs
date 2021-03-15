using System;

namespace CodeProject.ObjectPool.Specialized
{
	public sealed class MemoryStreamPool : ObjectPool<PooledMemoryStream>, IMemoryStreamPool, IObjectPool<PooledMemoryStream>
	{
		public const int DefaultMinimumMemoryStreamCapacity = 4096;

		public const int DefaultMaximumMemoryStreamCapacity = 524288;

		private int _minimumItemCapacity = 4096;

		private int _maximumItemCapacity = 524288;

		public static IMemoryStreamPool Instance
		{
			get;
		} = new MemoryStreamPool();


		public int MinimumMemoryStreamCapacity
		{
			get
			{
				return _minimumItemCapacity;
			}
			set
			{
				int minimumItemCapacity = _minimumItemCapacity;
				_minimumItemCapacity = value;
				if (minimumItemCapacity < value)
				{
					Clear();
				}
			}
		}

		public int MaximumMemoryStreamCapacity
		{
			get
			{
				return _maximumItemCapacity;
			}
			set
			{
				int maximumItemCapacity = _maximumItemCapacity;
				_maximumItemCapacity = value;
				if (maximumItemCapacity > value)
				{
					Clear();
				}
			}
		}

		public MemoryStreamPool()
			: base(16, (Func<PooledMemoryStream>)null)
		{
			base.FactoryMethod = (() => new PooledMemoryStream(MinimumMemoryStreamCapacity));
		}
	}
}
