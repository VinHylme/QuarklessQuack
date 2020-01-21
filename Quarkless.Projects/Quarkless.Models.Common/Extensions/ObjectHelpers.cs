using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.Models.Common.Extensions
{
	public static class ObjectHelpers
	{
		public static T GetAt<T>(this IEnumerable<T> items, int position) => items.ElementAtOrDefault(position);

		public static bool IsGreaterThan<T>(this IEnumerable<T> items, IEnumerable<T> target) =>
			items.Count() > target.Count();
		public static int TimesBy(this IEnumerable<int> values)
		{
			return values.Aggregate(0, (current, value) => current * value);
		}
		public static int CalculateTotalHash(this IEnumerable<object> values)
		{
			return TimesBy(values.Select(x => x.GetHashCode()));
		}
		public static int CalculateTotalHash(params object[] values)
		{
			return TimesBy(values.Select(x => x.GetHashCode()));
		}
		public static TResult IfNotNull<TInput, TResult>(this TInput o, Func<TInput, TResult> evaluator)
		where TResult : class where TInput : class
		{
			return o == null ? null : evaluator(o);
		}
		public static TInput CreateNewObjectIgnoringNulls<TInput>(this TInput @object, TInput target) where TInput : new()
		{
			var objectProperties = @object.GetType().GetProperties();
			var targetProperties = target.GetType().GetProperties();
			TInput @newObject = new TInput();

			for(int i = 0; i < targetProperties.Count(); i++)
			{
				var valueOfTargetProp = targetProperties.ElementAt(i).GetValue(target);
				var valueOfObjectProp = objectProperties.ElementAt(i).GetValue(@object);
				if(valueOfObjectProp==null && valueOfTargetProp != null)
				{
					newObject.GetType().GetProperties().ElementAt(i).SetValue(@newObject,valueOfTargetProp);
				}
				else if(valueOfObjectProp!=null && valueOfTargetProp == null)
				{
					newObject.GetType().GetProperties().ElementAt(i).SetValue(@newObject, valueOfObjectProp);
				}
				else if(valueOfObjectProp!=null && valueOfTargetProp != null)
				{
					newObject.GetType().GetProperties().ElementAt(i).SetValue(@newObject, valueOfObjectProp);
				}
			}
			return @newObject;
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
