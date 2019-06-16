﻿using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.MediaModels
{
	public class UploadVideoModel
	{
		public InstaVideoUpload Video { get; set; }
		public string Caption { get; set; }
		public InstaLocationShort Location {get;set; } = null;
	}
}
