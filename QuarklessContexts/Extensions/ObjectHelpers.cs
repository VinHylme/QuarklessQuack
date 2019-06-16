using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.Extensions
{
	public static class ObjectHelpers
	{
		public static TResult IfNotNull<TInput, TResult>(this TInput o, Func<TInput, TResult> evaluator)
		where TResult : class where TInput : class
		{
			if (o == null) return null;
			return evaluator(o);
		}
		public static Dictionary<string,object> Recreate<TInput>(this TInput objec)
		{
			var properties = objec.GetType().GetProperties();
			var newObject = new Dictionary<string,object>();
			foreach(var prop in properties)
			{
				var valueOfProp = prop.GetValue(objec);
				var attr = prop.CustomAttributes.Any(_=>_.AttributeType == typeof(BsonIdAttribute));
				if (valueOfProp != null && !attr)
				{
					newObject.Add(prop.Name, valueOfProp);
				}
			}
			return newObject;
		}
	}
}
