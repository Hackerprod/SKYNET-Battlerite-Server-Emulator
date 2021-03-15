using System.Threading;

namespace CodeProject.ObjectPool.Core
{
	public class ObjectPoolDiagnostics
	{
		private long _objectResetFailedCount;

		private long _poolObjectHitCount;

		private long _poolObjectMissCount;

		private long _poolOverflowCount;

		private long _returnedToPoolByResurrectionCount;

		private long _returnedToPoolCount;

		private long _totalInstancesCreated;

		private long _totalInstancesDestroyed;

		public bool Enabled
		{
			get;
			set;
		}

		public long TotalLiveInstancesCount => _totalInstancesCreated - _totalInstancesDestroyed;

		public long ObjectResetFailedCount => _objectResetFailedCount;

		public long ReturnedToPoolByResurrectionCount => _returnedToPoolByResurrectionCount;

		public long PoolObjectHitCount => _poolObjectHitCount;

		public long PoolObjectMissCount => _poolObjectMissCount;

		public long TotalInstancesCreated => _totalInstancesCreated;

		public long TotalInstancesDestroyed => _totalInstancesDestroyed;

		public long PoolOverflowCount => _poolOverflowCount;

		public long ReturnedToPoolCount => _returnedToPoolCount;

		public ObjectPoolDiagnostics()
		{
			Enabled = false;
		}

		protected internal virtual void IncrementObjectsCreatedCount()
		{
			if (Enabled)
			{
				Interlocked.Increment(ref _totalInstancesCreated);
			}
		}

		protected internal virtual void IncrementObjectsDestroyedCount()
		{
			if (Enabled)
			{
				Interlocked.Increment(ref _totalInstancesDestroyed);
			}
		}

		protected internal virtual void IncrementPoolObjectHitCount()
		{
			if (Enabled)
			{
				Interlocked.Increment(ref _poolObjectHitCount);
			}
		}

		protected internal virtual void IncrementPoolObjectMissCount()
		{
			if (Enabled)
			{
				Interlocked.Increment(ref _poolObjectMissCount);
			}
		}

		protected internal virtual void IncrementPoolOverflowCount()
		{
			if (Enabled)
			{
				Interlocked.Increment(ref _poolOverflowCount);
			}
		}

		protected internal virtual void IncrementResetStateFailedCount()
		{
			if (Enabled)
			{
				Interlocked.Increment(ref _objectResetFailedCount);
			}
		}

		protected internal virtual void IncrementObjectResurrectionCount()
		{
			if (Enabled)
			{
				Interlocked.Increment(ref _returnedToPoolByResurrectionCount);
			}
		}

		protected internal virtual void IncrementReturnedToPoolCount()
		{
			if (Enabled)
			{
				Interlocked.Increment(ref _returnedToPoolCount);
			}
		}
	}
}
