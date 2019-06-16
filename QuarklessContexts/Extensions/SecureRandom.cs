using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace QuarklessContexts.Extensions
{
	public class SecureRandom : RandomNumberGenerator
	{
		private static readonly RandomNumberGenerator _rng = new RNGCryptoServiceProvider();
		public static int Next()
		{
			lock (_rng)
			{
				var data = new byte[sizeof(int)];
				_rng.GetBytes(data);
				return BitConverter.ToInt32(data,0) & (int.MaxValue - 1);
			}
			_rng.Dispose();
		}
		public static int Next(int maxValue)
		{
			return Next(0, maxValue);
		}

		public static int Next(int minValue, int maxValue)
		{
			if (minValue > maxValue)
			{
				throw new ArgumentOutOfRangeException();
			}
			return (int)Math.Floor((minValue + ((double)maxValue - minValue) * NextDouble()));
		}
		public static double NextDouble()
		{
			lock (_rng) { 
				var data = new byte[sizeof(uint)];
				_rng.GetBytes(data);
				var randUint = BitConverter.ToUInt32(data, 0);
				return randUint / (uint.MaxValue + 1.0);
			}
			_rng.Dispose();
		}

		public override void GetBytes(byte[] data)
		{
			_rng.GetBytes(data);
		}
	}
}
