using System;
using System.Threading.Tasks;

namespace Quarkless.Interfacing
{
	/// <summary>
	/// Trying out new thing, make this the base of everything, to make life easier with logging, and other things
	/// </summary>
	public abstract class CommonInterface
	{
		protected CommonInterface()
		{

		}
		public void Print(object message) => Console.WriteLine(message);
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
