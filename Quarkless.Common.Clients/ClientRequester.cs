using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common.Clients.Configs;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.SecurityLayerModels;

namespace Quarkless.Common.Clients
{
	public class ClientRequester
	{
		private readonly Socket _clientSocket;
		public ClientRequester()
		{
			_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public async Task<bool> TryConnect()
		{
			var attempts = 0;
			while (!_clientSocket.Connected && attempts < 15)
			{
				try
				{
					_clientSocket.Connect("quarkless.net.security", 65115);
					return true;
				}
				catch (SocketException se)
				{
					Console.WriteLine($"Connection failed: {se.Message}");
					attempts++;
				}
				await Task.Delay(TimeSpan.FromSeconds(2));
			}
			return false;
		}
		public void TryDisconnect()
		{
			try
			{
				if (!_clientSocket.Connected) return;
				_clientSocket.Disconnect(false);
				_clientSocket.Close();
				_clientSocket.Dispose();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		private IServiceCollection BuildServices(in EnvironmentsAccess access, params ServiceTypes[] serviceTypes)
			=> Config.BuildServices(access, serviceTypes);

		public object Send(object dataToSend)
		{
			try
			{
				var request = dataToSend.Serialize();
				var buffer = Encoding.ASCII.GetBytes(request);
				_clientSocket.Send(buffer);
				var bytesReceived = new byte[4096];
				var requestReceivedLen = _clientSocket.Receive(bytesReceived);
				var data = new byte[requestReceivedLen];
				Array.Copy(bytesReceived, data, requestReceivedLen);
				var response = Encoding.ASCII.GetString(data);
				switch (dataToSend)
				{
					case InitCommandArgs arg:
						return response == "Validated;";
					case BuildCommandArgs arg:
						var access = response.Deserialize<EnvironmentsAccess>();
						return access != null ? BuildServices(access, arg.ServiceTypes) : null;
					case GetPublicEndpointCommandArgs arg:
						return response.Deserialize<EndPoints>();
					default:
						throw new Exception("Invalid Command");
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
	}
}
