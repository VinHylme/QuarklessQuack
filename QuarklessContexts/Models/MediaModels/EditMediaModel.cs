using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.MediaModels
{
	public class EditMediaModel
	{
		public string Caption { get; set; }
		public InstaLocationShort Location { get;set; }  = null;
		public InstaUserTagUpload[] UserTags {get;set; } = null;
	}
}
