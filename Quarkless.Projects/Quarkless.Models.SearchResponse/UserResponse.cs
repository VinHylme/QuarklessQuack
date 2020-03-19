using System;
using Quarkless.Models.Common.Models.Topic;

namespace Quarkless.Models.SearchResponse
{
	[Serializable]
	public class UserResponse
	{
		public long UserId { get; set; }
		public string Username { get; set; }
		public string FullName { get; set; }
		public CTopic Topic { get; set; }
		public bool IsPrivate { get; set; }
		public bool IsVerified { get; set; }
		public string ProfilePicture { get; set; }
	}

	[Serializable]
	public class UserResponse<TObject>
	{
		public TObject Object { get; set; }
		public string MediaId { get; set; }
		public long UserId { get; set; }
		public string Username { get; set; }
		public string FullName { get; set; }
		public CTopic Topic { get; set; }
		public bool IsPrivate { get; set; }
		public bool IsVerified { get; set; }
		public string ProfilePicture { get; set; }
	}
}