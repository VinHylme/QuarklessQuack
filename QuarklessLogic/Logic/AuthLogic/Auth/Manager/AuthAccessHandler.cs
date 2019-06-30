using System;
using System.Text;

namespace QuarklessLogic.Logic.AuthLogic.Auth.Manager
{
	public class AuthAccessHandler : IAuthAccessHandler
	{
		private readonly string _iD;
		public AuthAccessHandler(string iD)
		{
			_iD = iD;
		}

		public string GetHash(string username,string clientId)
		{
			return GetSecretHash(username,clientId, _iD);
		}

		private string GetSecretHash(string username, string appClientId, string appSecretKey)
		{
			var dataString = username + appClientId;

			var data = Encoding.UTF8.GetBytes(dataString);
			var key = Encoding.UTF8.GetBytes(appSecretKey);

			return Convert.ToBase64String(HmacSHA256(data, key));
		}
		private byte[] HmacSHA256(byte[] data, byte[] key)
		{
			using (var shaAlgorithm = new System.Security.Cryptography.HMACSHA256(key))
			{
				var result = shaAlgorithm.ComputeHash(data);
				return result;
			}
		}
	}
}
