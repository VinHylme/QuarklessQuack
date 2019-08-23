using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Quarkless.Filters
{
	public class AddHeaderAttribute : ResultFilterAttribute
	{
		private readonly string _name;
		private readonly string _value;

		public AddHeaderAttribute(string name, string value = null)
		{
			_name = name;
			_value = !string.IsNullOrEmpty(value) ? value : Guid.NewGuid().ToString();
		}

		public override void OnResultExecuting(ResultExecutingContext context)
		{
			context.HttpContext.Response.Headers.Add( _name, new string[] { _value });
			base.OnResultExecuting(context);
		}
	}
}
