using System;
using System.Net.Sockets;
using System.Text;

namespace Quarkless.Logic.Security.Extensions
{
	internal static class SocketExtensions
	{
		internal static bool SendResponse(this Socket @socket, string resp)
		{
			try
			{
				var encoded = Encoding.ASCII.GetBytes(resp);
				@socket.BeginSend(encoded, 0, encoded.Length, SocketFlags.None,
					ar => { ((Socket)ar.AsyncState).EndSend(ar); },
					@socket);
				return true;
			}
			catch (SocketException socketException)
			{
				Console.WriteLine(socketException.Message);
				return false;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return false;
			}
		}
	}
}