using System;

namespace CodeProject.ObjectPool
{
	public static class PooledObjectWrapper
	{
		public static PooledObjectWrapper<T> Create<T>(T resource) where T : class
		{
			return new PooledObjectWrapper<T>(resource);
		}
	}
	[Serializable]
	public sealed class PooledObjectWrapper<T> : PooledObject where T : class
	{
		public T InternalResource
		{
			get;
		}

		public new Action<T> OnReleaseResources
		{
			get;
			set;
		}

		public new Action<T> OnResetState
		{
			get;
			set;
		}

		public PooledObjectWrapper(T resource)
		{
			if (resource == null)
			{
				throw new ArgumentNullException("resource", "Resource cannot be null.");
			}
			InternalResource = resource;
			base.OnReleaseResources = (Action)Delegate.Combine(base.OnReleaseResources, (Action)delegate
			{
				OnReleaseResources?.Invoke(InternalResource);
			});
			base.OnResetState = (Action)Delegate.Combine(base.OnResetState, (Action)delegate
			{
				OnResetState?.Invoke(InternalResource);
			});
		}
	}
}
