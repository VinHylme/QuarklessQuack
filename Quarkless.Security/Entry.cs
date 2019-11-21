using System;

namespace Quarkless.Security
{
	public class Entry
	{
		static void Main(string[] args)
		{
			var server = new ServerListener.ServerNetwork();
			server.StartAccepting();
			Console.Read();
		}
	}
}
