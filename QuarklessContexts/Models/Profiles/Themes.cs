using System.Collections.Generic;

namespace QuarklessContexts.Models.Profiles
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