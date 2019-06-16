using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessLogic.Logic.CommentLogic;
using QuarklessLogic.Logic.DiscoverLogic;
using QuarklessRepositories.Repository.ServicesRepositories.CommentsRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quarkless.Services.CommentsServices
{
	public class CommentsServices : ICommentsServices
	{
		private readonly IDiscoverLogic _discoverLogic;
		private readonly ICommentLogic _commentLogic;
		private readonly ICommentsRepository _commentsRepository;
		public CommentsServices(IDiscoverLogic discoverLogic, ICommentLogic commentLogic, ICommentsRepository commentsRepository)
		{
			_discoverLogic = discoverLogic;
			_commentLogic = commentLogic;
			_commentsRepository = commentsRepository;
		}

		public async Task<bool> FetchComments(string topic, int limit = 20)
		{
			try
			{
				var discover = await _discoverLogic.GetTopHashtagMediaList(topic, limit);
				//call the captiondb and store in there
				//the media caption
				if (discover.Succeeded)
				{
					List<CommentsModel> Comments = new List<CommentsModel>();
					var discoverResults = discover.Value.Medias;
					foreach (var media in discoverResults)
					{
						var commentsOf = await _commentLogic.GetMediaCommentsAsync(media.Pk, limit * 4);
						if (commentsOf.Succeeded && commentsOf.Value?.Comments?.Count > 0)
						{
							var comments_ = commentsOf.Value;

							foreach (var com in comments_.Comments)
							{
								if (com.Text.Count(_ => _ == '#') > 1) { continue; }
								else
								{
									CommentsModel comment = new CommentsModel();
									comment.MediaId = media.Pk;
									comment.Topic = topic;
									comment.Media_CommentCount = int.Parse(media.CommentsCount ?? "0");
									comment.Media_LikesCount = media.LikesCount;
									comment.Media_ViewsCount = media.ViewCount;
									comment.User_Id = com.User.Pk;
									comment.User_Username = com.User.UserName;
									comment.Text = com.Text;
									comment.Comment_LikesCount = com.LikesCount;
									comment.Comment_Replies = com.ChildCommentCount;
									comment.InstaCommentId = com.Pk;
									comment.CommentMadeAt = com.CreatedAtUtc;

									foreach (var replies in com.PreviewChildComments)
									{
										comment.PreviewReplies.Add(new Reply
										{
											CommentMadeAt = replies.CreatedAtUtc,
											Comment_LikesCount = replies.CommentLikeCount,
											InstaCommentId = replies.Pk,
											Text = replies.Text,
											User_Id = replies.User.Pk,
											User_Username = replies.User.UserName
										});
									}

									Comments.Add(comment);
								}
							}
						}
						Thread.Sleep(TimeSpan.FromSeconds(5));
					}
					if (Comments.Count > 0)
					{
						_commentsRepository.AddComments(Comments);
						return true;
					}
					return false;
				}
				return false;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return false;
			}
		}

	}
}
