namespace Quarkless.Models.RequestBuilder.Interfaces
{
	public interface IUrlReader
	{
		string CreateComment { get; }
		string LikeComment { get; }
		string UnlikeComment { get; }
		string TranslateComment { get; }
		string ReplyComment { get; }
		string UploadCarousel { get; }
		string UploadPhoto { get; }
		string UploadVideo { get; }
		string DeleteMedia { get; }
		string FollowUser { get; }
		string UnfollowUser { get; }
		string LikeMedia { get; }
		string SendDirectMessageText { get; }
		string SendDirectMessageLink { get; }
		string SendDirectMessagePhoto { get; }
		string SendDirectMessageMedia { get; }
		string SendDirectMessageMediaWithThreads { get; }
		string SendDirectMessageVideo { get; }
		string SendDirectMessageProfile { get; }
	}
}