using System;

namespace QuarklessContexts.Models.Options
{
	public class RedisOptions
	{
		public string ConnectionString { get; set; }
		public int DatabaseNumber { get; set; } = 0;
		public TimeSpan DefaultKeyExpiry { get; set; }
	}
}
