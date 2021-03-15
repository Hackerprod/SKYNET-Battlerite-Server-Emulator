using System;

namespace CodeProject.ObjectPool.Logging.LogProviders
{
	internal class DisposableAction : IDisposable
	{
		private readonly Action _onDispose;

		public DisposableAction(Action onDispose = null)
		{
			_onDispose = onDispose;
		}

		public void Dispose()
		{
			if (_onDispose != null)
			{
				_onDispose();
			}
		}
	}
}
