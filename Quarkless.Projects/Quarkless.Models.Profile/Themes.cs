using Quarkless.Models.SearchResponse;
using System.Collections.Generic;

namespace Quarkless.Models.Profile
{
	public class Themes
	{
		public string Name { get; set; }
		public List<Color> Colors { get; set; }
		public List<GroupImagesAlike> ImagesLike { get; set; }
		public double Percentage { get; set; }
		public Themes()
		{
			
		}
	}
}