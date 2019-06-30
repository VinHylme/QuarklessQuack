using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessRepositories.RedisRepository.RedisClient
{
	public class RedisOptions
	{
		public string ConnectionString { get; set; }
		public TimeSpan DefaultKeyExpiry { get; set; }
	}
}
