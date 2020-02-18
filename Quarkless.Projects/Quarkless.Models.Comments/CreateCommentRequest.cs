﻿using Quarkless.Models.Common.Interfaces;
using Quarkless.Models.Common.Models.Resolver;

namespace Quarkless.Models.Comments
{
	public class CreateCommentRequest : IExec
	{
		public string Text { get; set; }
		public MediaShort Media { get; set; }
		public UserShort User { get; set; }
		public DataFrom DataFrom { get; set; }
	}
}