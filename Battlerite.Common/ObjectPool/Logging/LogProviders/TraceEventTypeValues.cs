using System;
using System.Reflection;

namespace CodeProject.ObjectPool.Logging.LogProviders
{
	internal static class TraceEventTypeValues
	{
		internal static readonly Type Type;

		internal static readonly int Verbose;

		internal static readonly int Information;

		internal static readonly int Warning;

		internal static readonly int Error;

		internal static readonly int Critical;

		static TraceEventTypeValues()
		{
			Assembly assemblyPortable = typeof(Uri).GetAssemblyPortable();
			if (!(assemblyPortable == null))
			{
				Type = assemblyPortable.GetType("System.Diagnostics.TraceEventType");
				if (!(Type == null))
				{
					Verbose = (int)Enum.Parse(Type, "Verbose", ignoreCase: false);
					Information = (int)Enum.Parse(Type, "Information", ignoreCase: false);
					Warning = (int)Enum.Parse(Type, "Warning", ignoreCase: false);
					Error = (int)Enum.Parse(Type, "Error", ignoreCase: false);
					Critical = (int)Enum.Parse(Type, "Critical", ignoreCase: false);
				}
			}
		}
	}
}
