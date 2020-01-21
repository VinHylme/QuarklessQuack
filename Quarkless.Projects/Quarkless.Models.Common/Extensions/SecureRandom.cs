using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Quarkless.Models.Common.Extensions
{
	public class Chance<TObject>
	{
		public TObject @Object { get; set; }
		public double Probability { get; set; }
	}
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
		}
		public static TObject ProbabilityRoll <TObject>(IEnumerable<Chance<TObject>> objects)
		{
			var rand = NextDouble() * objects.Sum(_=>_.Probability);
			double value = 0;
			foreach(var obj in objects)
			{
				value += obj.Probability;
				if (rand <= value)
				{
					return obj.Object;
				}
			}
			return default(TObject);
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
		}

		public override void GetBytes(byte[] data)
		{
			_rng.GetBytes(data);
		}
	}
}
