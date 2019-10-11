using QuarklessContexts.Models.Topics;
using System.Collections.Generic;

namespace QuarklessContexts.Models.QueryModels.Settings
{
	public class ProfileConfiguration
	{
		public IEnumerable<TopicCategories> Topics;
		public Dictionary<string,string> Languages;
		public IEnumerable<string> ColorsAllowed;
		public bool CanUserEditProfile = true;
		public IEnumerable<string> ImageTypes;
		public IEnumerable<string> Orientations;
		public IEnumerable<string> SizeTypes;
		public IEnumerable<string> SearchTypes;
		public ProfileConfiguration()
		{
		}
	}
	public class ProfileSettings
	{
	}
}
