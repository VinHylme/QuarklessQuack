using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.RequestBuilder.Consts
{
	public class ActionTrigger
	{
		public ActionInstances Action { get; set; }
		public ContentType Content { get; set; }
	}
	public class UrlConstants
	{
		private const string BasePath = "http://localhost:51518/";
		private const string ApiPath = BasePath+ "api/";
		
		#region Comments
		public const string CreateComment = ApiPath + "comments/create/{mediaId}";
		public const string LikeComment = ApiPath + "comments/like/{commentId}";
		public const string UnlikeComment = ApiPath + "comments/unlike/{commentId}";
		public const string TranslateComment = ApiPath + "comments/translate";
		public const string ReplyComment = ApiPath + "comments/reply/{mediaId}/{targetCommentId}";
		#endregion
		#region Media
		public const string UploadPhoto = ApiPath + "media/upload/photo";
		#endregion
		public static string GetBasePath
		{
			get { return BasePath; }
		}
	}
}
