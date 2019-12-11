namespace QuarklessLogic.Handlers.RequestBuilder.Constants
{
	public class UrlReader : IUrlReader
	{
		private readonly string _apiBasePath;		

		public UrlReader(string apiBasePath)
		{
			_apiBasePath = apiBasePath;
		}
		
		#region Comments
		public string CreateComment => _apiBasePath + "comments/create/{0}";
		public string LikeComment => _apiBasePath + "comments/like/{0}";
		public string UnlikeComment => _apiBasePath + "comments/unlike/{0}";
		public string TranslateComment => _apiBasePath + "comments/translate";
		public string ReplyComment => _apiBasePath + "comments/reply/{0}/{1}";
		#endregion
		#region Media
		public string UploadCarousel => _apiBasePath + "media/upload/carousel";
		public string UploadPhoto => _apiBasePath + "media/upload/photo";
		public string UploadVideo => _apiBasePath + "media/upload/video";
		public string DeleteMedia => _apiBasePath + "/media/delete/{0}/{1}";
		#endregion
		#region Engage
		public string FollowUser => _apiBasePath + "instaUser/followUser/{0}";
		public string UnfollowUser => _apiBasePath + "instaUser/unFollowUser/{0}";
		public string LikeMedia => _apiBasePath + "media/like/{0}";
		public string SendDirectMessageText => _apiBasePath + "messaging/text";
		public string SendDirectMessageLink => _apiBasePath + "messaging/link";
		public string SendDirectMessagePhoto => _apiBasePath + "messaging/photo";
		public string SendDirectMessageMedia => _apiBasePath + "messaging/media";
		public string SendDirectMessageMediaWithThreads => _apiBasePath + "messaging/media-threads";
		public string SendDirectMessageVideo => _apiBasePath + "messaging/video";
		public string SendDirectMessageProfile => _apiBasePath + "messaging/profile";

		#endregion
	}
}
