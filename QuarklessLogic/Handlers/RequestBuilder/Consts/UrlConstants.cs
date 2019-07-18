using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessLogic.Handlers.RequestBuilder.Consts
{
	public class UrlConstants
	{
		private const string BasePath = "http://localhost:51518/";
		private const string ApiPath = BasePath+ "api/";
		
		#region Comments
		public const string CreateComment = ApiPath + "comments/create/{0}";
		public const string LikeComment = ApiPath + "comments/like/{0}";
		public const string UnlikeComment = ApiPath + "comments/unlike/{0}";
		public const string TranslateComment = ApiPath + "comments/translate";
		public const string ReplyComment = ApiPath + "comments/reply/{0}/{1}";
		#endregion
		#region Media
		public const string UploadCarousel = ApiPath + "media/upload/carousel";
		public const string UploadPhoto = ApiPath + "media/upload/photo";
		public const string UploadVideo = ApiPath + "media/upload/video";
		public const string DeleteMedia = ApiPath + "/media/delete/{0}/{1}";
		#endregion
		#region Engage
		public const string FollowUser = ApiPath + "instaUser/followUser/{0}";
		public const string UnfollowUser = ApiPath + "instaUser/unFollowUser/{0}";
		public const string LikeMedia = ApiPath + "media/like/{0}";
		#endregion
		public static string GetBasePath
		{
			get { return BasePath; }
		}
	}
}
