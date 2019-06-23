using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public class UserResponse<TObject>
	{
		public TObject Object { get; set; }
		public long UserId { get; set; }
		public string Username { get; set; }
		public string FullName { get; set; }
		public string Topic { get; set; }
		public bool IsPrivate { get; set; }
		public bool IsVerified { get; set; }
		public long FollowerCount { get; set; }
		public string ProfilePicture { get; set; }
	}
}
