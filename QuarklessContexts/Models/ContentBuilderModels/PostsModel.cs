using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ContentBuilderModels
{
	public class PostsModel
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public double Length { get; set; }
		public List<string> MediaData { get; set; }
	}
}
