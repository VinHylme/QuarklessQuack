using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.MediaModels
{
	public class UploadPhotoModel
	{
		public InstaImageUpload Image {get; set; }
		public string Caption {get;set; }
		public InstaLocationShort Location { get; set; } = null;
	}
}
