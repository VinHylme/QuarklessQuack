using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Quarkless.Interfacing.Objects;

namespace Quarkless.Interfacing
{
	public static class CommonExtensions
	{
		public static T GetAt<T>(this IEnumerable<T> items, int position) => items.ElementAtOrDefault(position);

		public static bool IsGreaterThan<T>(this IEnumerable<T> items, IEnumerable<T> target) =>
			items.Count() > target.Count();
	}

	/// <summary>
	/// Trying out new thing, make this the base of everything, to make life easier with logging, and other things
	/// </summary>
	public abstract class CommonInterface
	{
		protected CommonInterface()
		{
			var obList = new List<SString>();
			obList.GetAt(12);
			List(obList);
		}

		public List<T> List<T>(IEnumerable<T> items) => items.ToList();

		//public int Len<T>(T genericObject) => Marshal.SizeOf(genericObject);
		public int Len(string @string) => @string.Length;
		public int Len(IEnumerable<object> @array) => @array.Count();
		public long Len(object @object)
		{
			using (var stream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, @object);
				return stream.Length;
			}
		}
		
		public void Print(object message) => Console.Write(message);
		public void PrintLn(object message) => Console.WriteLine(message);

		/// <summary>
		/// Todo: add functionality for logger
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="code"></param>
		/// <param name="ids"></param>
		/// <returns></returns>
		public Task<T> RunCodeWithLoggerExceptionAsync<T>(Func<Task<T>> code, params string[] ids)
		{
			try
			{
				return code();
			}
			catch (Exception io)
			{
				Console.WriteLine(io.Message + string.Join(",", ids));
				throw;
			}
		}
		public Task<T> RunCodeWithExceptionAsync<T>(Func<Task<T>> code, params string[] ids)
		{
			try
			{
				return code();
			}
			catch (Exception io)
			{
				Console.WriteLine(io.Message + string.Join(",", ids));
				throw;
			}
		}
		public Task RunCodeWithExceptionAsync(Func<Task> code, params string[] ids)
		{
			try
			{
				return code();
			}
			catch (Exception io)
			{
				Console.WriteLine(io.Message + string.Join(",", ids));
				throw;
			}
		}
	}
}
