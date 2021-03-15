using CodeProject.ObjectPool.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CodeProject.ObjectPool
{
	public sealed class EvictionTimer : IEvictionTimer, IDisposable
	{

        private readonly Dictionary<Guid, Timer> _actionMap = new Dictionary<Guid, Timer>();

		private volatile bool _disposed;

		~EvictionTimer()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public Guid Schedule(Action action, TimeSpan delay, TimeSpan period)
		{
			ThrowIfDisposed();
			if (action == null)
			{
				return Guid.Empty;
			}
			lock (_actionMap)
			{
				TimerCallback callback = delegate
				{
					action();
				};
				Guid guid = Guid.NewGuid();
				_actionMap[guid] = new Timer(callback, null, delay, period);
				return guid;
			}
		}

		public void Cancel(Guid actionTicket)
		{
			ThrowIfDisposed();
			lock (_actionMap)
			{
				if (_actionMap.TryGetValue(actionTicket, out Timer value))
				{
					_actionMap.Remove(actionTicket);
					value.Dispose();
				}
			}
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_disposed = true;
				if (disposing && _actionMap != null)
				{
					IEnumerable<Timer> enumerable = _actionMap.Values.ToArray();
					IEnumerable<Timer> obj = enumerable ?? Enumerable.Empty<Timer>();
					_actionMap.Clear();
					foreach (Timer item in obj)
					{
						item.Dispose();
					}
				}
			}
		}
	}
}
