using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessRepositories.RedisRepository.RedisClient
{
	public class RedisKeys
	{
		public enum HashtagGrowKeys
		{
			Timeline,
			Usersessions,
			MetaData,
			Timelinejob,
			Corpus,
			SearchSession,
			UserLibrary
		}
	}
}
