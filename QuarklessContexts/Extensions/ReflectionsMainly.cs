using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace QuarklessContexts.Extensions
{
	public static class ReflectionsMainly
	{
		public static IEnumerable<Type> GetNumberOfClassesDerivedFromInterface(this Type @interface)
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes().Where(@interface.IsAssignableFrom));
		}
		public static object TryConvertObjectOfInterfaceType(this string jsonString, Type @interface)
		{
			var typesAvailable = @interface.GetNumberOfClassesDerivedFromInterface();
			foreach (var type in typesAvailable)
			{
				//try to convert
				try
				{
					return JsonConvert.DeserializeObject(jsonString, type, new JsonSerializerSettings
					{
						MissingMemberHandling = MissingMemberHandling.Error
					});
				}
				catch
				{
					// ignored
				}
			}

			return null;
		}
		public static object TryConvertObjectOfInterfaceType(this byte[] bytes, Type @interface)
		{
			var typesAvailable = @interface.GetNumberOfClassesDerivedFromInterface();
			foreach (var type in typesAvailable)
			{
				//try to convert
				try
				{
					return bytes.Deserialize(type);
				}
				catch
				{
					// ignored
				}
			}

			return null;
		}
	}
}
