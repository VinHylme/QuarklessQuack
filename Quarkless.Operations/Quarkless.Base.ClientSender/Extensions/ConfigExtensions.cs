using System;
using System.Linq;
using Quarkless.Base.ClientSender.Extensions.Configs;

namespace Quarkless.Base.ClientSender.Extensions
{
	public static class ConfigExtensions
	{
		public static bool ObjectIncludedInClass(this string methodName, Type @class)
		{
			return @class.GetMethods().Any(obj => obj.Name.ToLower().Equals(methodName.ToLower()));
		}
		public static int MethodParamLength(this string methodName)
		{
			var @class = typeof(ConfigureServices);
			var method = @class.GetMethod(methodName);
			if (method != null) return method.GetParameters().Length;
			return -1;
		}
		public static void InvokeConfigureMethod(this string methodName, params object[] parameters)
		{
			var @class = typeof(ConfigureServices);
			var method = @class.GetMethod(methodName);
			if (method == null) return;
			method.Invoke(@class, parameters);
		}
	}
}
