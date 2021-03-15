using System;

namespace CodeProject.ObjectPool.Specialized
{
	public sealed class StringBuilderPool : ObjectPool<PooledStringBuilder>, IStringBuilderPool, IObjectPool<PooledStringBuilder>
	{
		public const int DefaultMinimumStringBuilderCapacity = 4096;

		public const int DefaultMaximumStringBuilderCapacity = 524288;

		private int _minimumItemCapacity = 4096;

		private int _maximumItemCapacity = 524288;

		public static IStringBuilderPool Instance
		{
			get;
		} = new StringBuilderPool();


		public int MinimumStringBuilderCapacity
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

		public int MaximumStringBuilderCapacity
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

		public StringBuilderPool()
			: base(16, (Func<PooledStringBuilder>)null)
		{
			base.FactoryMethod = (() => new PooledStringBuilder(MinimumStringBuilderCapacity));
		}

		public PooledStringBuilder GetObject(string value)
		{
			PooledStringBuilder @object = GetObject();
			@object.StringBuilder.Append(value);
			return @object;
		}
	}
}
