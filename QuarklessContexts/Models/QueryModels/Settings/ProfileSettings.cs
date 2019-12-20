﻿using System;
using System.Collections.Generic;
using QuarklessContexts.Models.Profiles;

namespace QuarklessContexts.Models.QueryModels.Settings
{
	[Serializable]
	public class ProfileConfiguration
	{
		public IEnumerable<Topic> Topics { get; set; }
		public Dictionary<string,string> Languages { get; set; }
		public IEnumerable<string> ColorsAllowed { get; set; }
		public bool CanUserEditProfile { get; set; } = true;
		public IEnumerable<string> ImageTypes { get; set; }
		public IEnumerable<string> Orientations { get; set; }
		public IEnumerable<string> SizeTypes { get; set; }
		public IEnumerable<string> SearchTypes { get; set; }
	}
}
