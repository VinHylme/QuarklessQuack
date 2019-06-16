using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using QuarklessLogic.Logic.HashtagLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quarkless.Services
{
	public class SearchServices : ISearchServices
	{
		private readonly IHashtagLogic _hashtagLogic;
		public SearchServices(IHashtagLogic hashtagLogic)
		{
			_hashtagLogic = hashtagLogic;
		}

		public async Task<IEnumerable<string>> SearchTag(string query, IEnumerable<long> exclude = null, string rankToken = null)
		{
			var results = await _hashtagLogic.SearchHashtagAsync(query, exclude, rankToken);
			if (results.Succeeded)
			{
				return results.Value.Select(_ => _.Name);
			}
			return null;
		}
	}
}
