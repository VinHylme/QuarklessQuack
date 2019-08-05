using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Quarkless.Filters
{
	public class Encrypter
	{
		//private const string _key = "Sherox.101";
		public byte[] _PublicKey;
		private byte[] _Key;

		public Encrypter(byte[] senderPublicKey)
		{
			//using (ECDiffieHellmanCng encr = new ECDiffieHellmanCng())
			//{
			//	encr.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
			//	encr.HashAlgorithm = CngAlgorithm.Sha256;
			//	_PublicKey = encr.PublicKey.ToByteArray();
			//	_Key = encr.DeriveKeyMaterial(CngKey.Import(senderPublicKey, CngKeyBlobFormat.EccPublicBlob));
			//}
			//using (ECDiffieHellman alice = ECDiffieHellman.Create(ECCurve.NamedCurves.brainpoolP256r1))
			//{
			//	var alicePublicKey = Convert.ToBase64String(alice.PublicKey.ToByteArray());
			//	//send alicePublicKey
			//	var nodejsKey = ""; //NODEJS brainpoolP256r1 publickey  base64
			//	byte[] nodejsKeyBytes = Convert.FromBase64String(nodejsKey);

			//	var aliceKey = Convert.ToBase64String(getDeriveKey(nodejsKeyBytes, alice));
			//	byte[] encryptedMessage = null;
			//	byte[] iv = null;
			//	// Send(aliceKey, "Secret message", out encryptedMessage, out iv);
			//}
			using(var rijndael = new RijndaelManaged())
			{
				rijndael.GenerateIV();
				rijndael.GenerateKey();
				var l = System.Text.Encoding.Default.GetString(rijndael.Key);
				var m = System.Text.Encoding.Default.GetString(rijndael.IV);
			}
		}
		private byte[] getDeriveKey(byte[] key1, ECDiffieHellman alice)
		{
			byte[] keyX = new byte[key1.Length / 2];
			byte[] keyY = new byte[keyX.Length];
			Buffer.BlockCopy(key1, 1, keyX, 0, keyX.Length);
			Buffer.BlockCopy(key1, 1 + keyX.Length, keyY, 0, keyY.Length);
			ECParameters parameters = new ECParameters
			{
				Curve = ECCurve.NamedCurves.brainpoolP256r1,
				Q =
			{
				X = keyX,
				Y = keyY,
			},
			};
			byte[] derivedKey;
			using (ECDiffieHellman bob = ECDiffieHellman.Create(parameters))
			using (ECDiffieHellmanPublicKey bobPublic = bob.PublicKey)
			{
				return derivedKey = alice.DeriveKeyFromHash(bobPublic, HashAlgorithmName.SHA256);
			}
		}
		public string Decrypt(string message)
		{
			string keyString = "D1C6ED3C6049221408A4EB634F20E393270ABC31145C20A6"; //replace with your key
			string ivString = "A80198517884F820FE558857688728CE"; //replace with your iv

			byte[] key = Encoding.ASCII.GetBytes(keyString);
			byte[] iv = Encoding.ASCII.GetBytes(ivString);

			using (var rijndaelManaged =
					new RijndaelManaged { Key = key, IV = iv, Mode = CipherMode.CBC })
			{
				rijndaelManaged.BlockSize = 128;
				rijndaelManaged.KeySize = 256;
				using (var memoryStream =
					   new MemoryStream(Convert.FromBase64String(message)))
				using (var cryptoStream =
					   new CryptoStream(memoryStream,
						   rijndaelManaged.CreateDecryptor(key, iv),
						   CryptoStreamMode.Read))
				{
					return new StreamReader(cryptoStream).ReadToEnd();
				}
			}
		}
		public string Receive(byte[] encryptedMessage, byte[] iv)
		{
			using (Aes aes = new AesCryptoServiceProvider())
			{
				aes.Key = _Key;
				aes.IV = iv;

				using (MemoryStream plaintext = new MemoryStream())
				{
					using (CryptoStream cs = new CryptoStream(plaintext, aes.CreateDecryptor(), CryptoStreamMode.Write))
					{
						cs.Write(encryptedMessage, 0, encryptedMessage.Length);
						cs.Close();
						string message = Encoding.UTF8.GetString(plaintext.ToArray());
						return message;
					}
				}
			}
		}
	}
}
