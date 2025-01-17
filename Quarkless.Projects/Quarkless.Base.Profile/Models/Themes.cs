﻿using System.Collections.Generic;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Base.Profile.Models
{
	public class Themes
	{
		public string Name { get; set; }
		public List<Color> Colors { get; set; }
		public List<GroupImagesAlike> ImagesLike { get; set; }
		public double Percentage { get; set; }
	}
}