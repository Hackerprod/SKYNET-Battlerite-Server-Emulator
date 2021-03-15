using System;

namespace CodeProject.ObjectPool.Logging
{
	public interface ILogProvider
	{
		Logger GetLogger(string name);

		IDisposable OpenNestedContext(string message);

		IDisposable OpenMappedContext(string key, string value);
	}
}
