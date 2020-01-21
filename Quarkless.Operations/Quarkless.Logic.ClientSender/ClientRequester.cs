using Quarkless.Models.ClientSender;
using Quarkless.Models.Common.Extensions;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.ClientSender.Enums;

namespace Quarkless.Logic.ClientSender
{
	public class ClientRequester
	{
		private const string SERVER_IP = "security.quark";
		private readonly Socket _clientSocket;
		private readonly string _host = SERVER_IP;
		private const int BYTE_LIMIT = 8192;
		private readonly bool _useLocalHost;
		public ClientRequester(bool useLocalHostHost = false)
		{
			_useLocalHost = useLocalHostHost;

			if (_useLocalHost)
				_host = "localhost";

			_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public async Task<bool> TryConnect()
		{
			var attempts = 0;
			while (!_clientSocket.Connected && attempts < 15)
			{
				try
				{
					_clientSocket.Connect(_host, 65115);
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
				_clientSocket.Shutdown(SocketShutdown.Both);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				_clientSocket.Close();
				_clientSocket.Dispose();
			}
		}
		private IServiceCollection BuildServices(in EnvironmentsAccess access, params ServiceTypes[] serviceTypes)
			=> Config.BuildServices(access, serviceTypes);

		public IServiceCollection Build(AvailableClient client, params ServiceTypes[] servicesToBuild)
		{
			try
			{
				var argData = new ArgData
				{
					UseLocal = _useLocalHost,
					Client = client,
					Services = servicesToBuild
				};
				var request = Encoding.ASCII.GetBytes(argData.Serialize());

				_clientSocket.Send(request);

				var bytesReceived = new byte[BYTE_LIMIT];
				var requestReceivedLen = _clientSocket.Receive(bytesReceived);
				var data = new byte[requestReceivedLen];
				Array.Copy(bytesReceived, data, requestReceivedLen);

				var response = Encoding.ASCII.GetString(data);

				return response != null
					? BuildServices(response.Deserialize<EnvironmentsAccess>(), servicesToBuild)
					: null;
			}
			catch(Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
		public EndPoints GetPublicEndPoints(GetPublicEndpointCommandArgs args)
		{
			try
			{
				args.UseLocal = _useLocalHost;
				var request = Encoding.ASCII.GetBytes(args.Serialize());
				_clientSocket.Send(request);
				var bytesReceived = new byte[BYTE_LIMIT];
				var requestReceivedLen = _clientSocket.Receive(bytesReceived);
				var data = new byte[requestReceivedLen];
				Array.Copy(bytesReceived, data, requestReceivedLen);

				var response = Encoding.ASCII.GetString(data);

				return response.Deserialize<EndPoints>();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
	}
}
