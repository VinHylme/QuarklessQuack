using System;
using System.Collections.Generic;
using Quarkless.Models.Topic;

namespace Quarkless.Models.Query
{
	[Serializable]
	public class ProfileConfiguration
	{
		public IEnumerable<CTopic> Categories { get; set; }
		public Dictionary<string, string> Languages { get; set; }
		public IEnumerable<string> ColorsAllowed { get; set; }
		public bool CanUserEditProfile { get; set; } = true;
		public IEnumerable<string> ImageTypes { get; set; }
		public IEnumerable<string> Orientations { get; set; }
		public IEnumerable<string> SizeTypes { get; set; }
		public IEnumerable<string> SearchTypes { get; set; }
	}
}