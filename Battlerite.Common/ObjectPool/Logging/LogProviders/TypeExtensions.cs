using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeProject.ObjectPool.Logging.LogProviders
{
	internal static class TypeExtensions
	{
		internal static ConstructorInfo GetConstructorPortable(this Type type, params Type[] types)
		{
			return type.GetConstructor(types);
		}

		internal static MethodInfo GetMethodPortable(this Type type, string name)
		{
			return type.GetMethod(name);
		}

		internal static MethodInfo GetMethodPortable(this Type type, string name, params Type[] types)
		{
			return type.GetMethod(name, types);
		}

		internal static PropertyInfo GetPropertyPortable(this Type type, string name)
		{
			return type.GetProperty(name);
		}

		internal static IEnumerable<FieldInfo> GetFieldsPortable(this Type type)
		{
			return type.GetFields();
		}

		internal static Type GetBaseTypePortable(this Type type)
		{
			return type.BaseType;
		}

		internal static object CreateDelegate(this MethodInfo methodInfo, Type delegateType)
		{
			return Delegate.CreateDelegate(delegateType, methodInfo);
		}

		internal static Assembly GetAssemblyPortable(this Type type)
		{
			return type.Assembly;
		}
	}
}
