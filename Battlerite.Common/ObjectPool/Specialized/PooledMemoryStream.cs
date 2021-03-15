using CodeProject.ObjectPool.Core;
using CodeProject.ObjectPool.Logging;
using System;
using System.IO;

namespace CodeProject.ObjectPool.Specialized
{
	public class PooledMemoryStream : PooledObject
	{
		private sealed class TrackedMemoryStream : MemoryStream
		{
			public PooledMemoryStream Parent
			{
				get;
				set;
			}

			public TrackedMemoryStream(int capacity)
				: base(capacity)
			{
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && Parent != null)
				{
					Parent.Dispose();
				}
				else
				{
					base.Dispose(disposing);
				}
			}
		}

        private readonly TrackedMemoryStream _trackedMemoryStream;

		public MemoryStream MemoryStream => _trackedMemoryStream;

		public PooledMemoryStream(int capacity)
		{
			_trackedMemoryStream = new TrackedMemoryStream(capacity)
			{
				Parent = this
			};
			base.OnValidateObject = (Func<PooledObjectValidationContext, bool>)Delegate.Combine(base.OnValidateObject, (Func<PooledObjectValidationContext, bool>)delegate(PooledObjectValidationContext ctx)
			{
				if (ctx.Direction == PooledObjectDirection.Outbound)
				{
					return true;
				}
				if (!_trackedMemoryStream.CanRead || !_trackedMemoryStream.CanWrite || !_trackedMemoryStream.CanSeek)
				{
					return false;
				}
				IMemoryStreamPool memoryStreamPool = base.PooledObjectInfo.Handle as IMemoryStreamPool;
				if (_trackedMemoryStream.Capacity < memoryStreamPool.MinimumMemoryStreamCapacity)
				{
					return false;
				}
				if (_trackedMemoryStream.Capacity > memoryStreamPool.MaximumMemoryStreamCapacity)
				{
					return false;
				}
				return true;
			});
			base.OnResetState = (Action)Delegate.Combine(base.OnResetState, (Action)delegate
			{
				_trackedMemoryStream.Position = 0L;
				_trackedMemoryStream.SetLength(0L);
			});
			base.OnReleaseResources = (Action)Delegate.Combine(base.OnReleaseResources, (Action)delegate
			{
				_trackedMemoryStream.Parent = null;
				_trackedMemoryStream.Dispose();
			});
		}

		public override string ToString()
		{
			return _trackedMemoryStream.ToString();
		}
	}
}
