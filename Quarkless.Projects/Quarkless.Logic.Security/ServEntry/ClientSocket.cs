using System.Net.Sockets;
using Quarkless.Models.ClientSender;
using Quarkless.Models.Common.Extensions;

namespace Quarkless.Logic.Security.ServEntry
{
	internal sealed class ClientSocket
	{
		private readonly ServerEntry _serverEntry;
		public Socket GetClientSocket { get; }
		public bool IsValidated => _serverEntry.IsValidated;
		public ClientSocket(Socket client)
		{
			GetClientSocket = client;
			_serverEntry = new ServerEntry();
		}

		public bool ValidateClient(AvailableClient availableClient) => _serverEntry.IsValidClient(availableClient);
		public string GetEnvData(bool useLocal) => _serverEntry.RequestEnvData(useLocal).Serialize();
		public string GetPublicEndpoints(bool useLocal) => _serverEntry.PublicEndpoints(useLocal).Serialize();
	}
}