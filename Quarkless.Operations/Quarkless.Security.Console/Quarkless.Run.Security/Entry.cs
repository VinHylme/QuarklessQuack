using Quarkless.Logic.Security;

namespace Quarkless.Run.Security
{
	public class Entry
	{
		static void Main(string[] args)
		{
			var server = new ServerNetwork();
			server.StartAccepting();
			while (true) { }
		}
	}
}
