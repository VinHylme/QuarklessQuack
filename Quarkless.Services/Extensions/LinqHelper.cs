using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Extensions
{
	public static class LinqHelperExtension
	{
		public static IEnumerable<TObject> SquashMe <TObject>(this IEnumerable<IEnumerable<TObject>> @items)
		{
			foreach(var item in @items)
			{
				foreach(var ite in item)
				{
					yield return ite;
				}
			}
		}
	}
}
