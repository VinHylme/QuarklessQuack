using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.QueryModels.Settings
{
	public class ProfileConfiguration
	{
		public IEnumerable<string> Topics;
		public Dictionary<string,string> Languages;
		public IEnumerable<string> ColorsAllowed;
		public bool CanUserEditProfile = true;
		public IEnumerable<string> ImageTypes;
		public IEnumerable<string> Orientations;
		public IEnumerable<string> SizeTypes;
		public ProfileConfiguration()
		{
		}
	}
	public class ProfileSettings
	{
	}
}
