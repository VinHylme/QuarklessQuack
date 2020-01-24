using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Quarkless.Utilities.Models.Extensions
{
	public static class UtilityExtensions
	{
		public static string Format(this string input, params string[] args)
		{
			var expression = @"\d{1}";
			var newString = input;

			var matches = Regex.Matches(input, expression);
			if (matches.Count <= 0)
				return newString;

			var argsNumbers = matches.Select(_ => int.Parse(_.Value));

			if (argsNumbers.Count() < args.Length || argsNumbers.Count() > args.Length)
				throw new Exception("Please make sure the arguments are and string replacement are the same length");

			if (argsNumbers.Any(pos => pos > args.Length || pos < 0))
				throw new NullReferenceException("Array Thrown Null Pointer Exception");

			foreach (var argPos in argsNumbers)
			{
				newString = Regex.Replace(newString, $"{argPos}", args[argPos]);
			}

			return newString;
		}
		public static List<T> TakeAny<T>(this IEnumerable<T> items, int amount, Random random)
		{
			if (items.Count() < amount)
			{
				amount = items.Count() - 1;
			}

			var uniqueItems = new List<T>();
			while (uniqueItems.Count < amount)
			{
				var item = items.ElementAtOrDefault(random.Next(items.Count()));
				if (!uniqueItems.Contains(item))
				{
					uniqueItems.Add(item);
				}
			}

			return uniqueItems;
		}

	}
}
