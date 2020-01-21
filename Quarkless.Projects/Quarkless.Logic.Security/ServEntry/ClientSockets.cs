using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Quarkless.Logic.Security.ServEntry
{
	internal sealed class ClientSockets : IEnumerable<ClientSocket>
	{
		private readonly List<ClientSocket> _clients;
		public ClientSockets()
		{
			_clients = new List<ClientSocket>();
		}

		public void Clean() => _clients.Clear();
		public void AddClient(ClientSocket client)
		{
			if(!_clients.Contains(client))
				_clients.Add(client);
		}
		public object this[int index] => _clients.ElementAtOrDefault(index);
		public ClientSocket GetClient(Socket client) => _clients.Find(c => c.GetClientSocket == client);
		public IEnumerator<ClientSocket> GetEnumerator()
		{
			return _clients.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}