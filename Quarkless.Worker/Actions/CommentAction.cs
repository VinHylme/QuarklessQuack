using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.Repository.ServicesRepositories;
using QuarklessRepositories.Repository.ServicesRepositories.CommentsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quarkless.Worker.Actions
{
	public class CommentAction : IWActions
	{
		private readonly string Topic;
		private readonly int Limit;
		private readonly IAPIClientContainer context_;
		private readonly IReportHandler _reportHandler;
		private ICommentsRepository _commentsRepository { get; set; }
		private IHashtagsRepository _hashtagsRepository { get; set; }

		public CommentAction(IAPIClientContainer context, IReportHandler reportHandler, string topic, int limit, params IServiceRepository[] serviceRepository)
		{
			this.Topic = topic;
			this.Limit = limit;
			this.context_ = context;
			this._reportHandler = reportHandler;
			foreach(var repo in serviceRepository)
			{
				if(repo is ICommentsRepository)
					this._commentsRepository = (ICommentsRepository) repo;
				if(repo is IHashtagsRepository)
					this._hashtagsRepository = (IHashtagsRepository) repo;
			}

			_reportHandler.SetupReportHandler("CommentActions", context.GetContext.InstagramAccount.AccountId, context.GetContext.InstagramAccount.Username);
		}

		public Task<object> Operate()
		{
			throw new NotImplementedException();
		}

		public async Task<bool> Operate(List<object> medias)
		{
			try
			{
				List<CommentsModel> Comments = new List<CommentsModel>();
				List<HashtagsModel> Hashtags = new List<HashtagsModel>();
				Console.WriteLine("BEGINING COMMENTS");
				for(int i = 0; i < medias.Count; i++)
				{
					InstaMedia media = (InstaMedia) medias[i];
					var commentsOf = await context_.Comment.GetMediaCommentsAsync(media.Pk, PaginationParameters.MaxPagesToLoad(Limit * 4));
					if (commentsOf.Succeeded && commentsOf.Value?.Comments?.Count > 0)
					{
						var comments_ = commentsOf.Value;

						foreach (var com in comments_.Comments)
						{
							if (com.Text.Count(_ => _ == '#') > 1) 
							{
								string[] hashtags_ = com.Text.Split('#');
								Hashtags.Add(new HashtagsModel
								{
									Hashtags = hashtags_.ToList().Select(a => a).ToList(),
									Topic = this.Topic,
								});

								continue; 
							}
							else
							{
								CommentsModel comment = new CommentsModel();
								comment.MediaId = media.Pk;
								comment.Topic = Topic;
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
					else
					{
						if (commentsOf.Info.ResponseType == ResponseType.RequestsLimit || commentsOf.Info.ResponseType == ResponseType.NetworkProblem)
						{
							return false;
						}
					}
				}
				if (Comments.Count > 0)
				{
					_commentsRepository.AddComments(Comments);
					await _hashtagsRepository.AddHashtags(Hashtags);
					return true;
				}
				return false;
			}
			catch (Exception ee)
			{

				_reportHandler.MakeReport(ee);
				return false;
			}
		}

		public async Task<bool> Operate(object item)
		{
			try
			{
				List<CommentsModel> Comments = new List<CommentsModel>();
				List<HashtagsModel> Hashtags = new List<HashtagsModel>();
				Console.WriteLine("BEGINING COMMENTS");
				InstaMedia media = (InstaMedia)item;
				var commentsOf = await context_.Comment.GetMediaCommentsAsync(media.Pk, PaginationParameters.MaxPagesToLoad(Limit * 4));
				if (commentsOf.Succeeded && commentsOf.Value?.Comments?.Count > 0)
				{
					var comments_ = commentsOf.Value;

					foreach (var com in comments_.Comments)
					{
						if (com.Text.Count(_ => _ == '#') > 1)
						{
							string[] hashtags_ = com.Text.Split('#');
							Hashtags.Add(new HashtagsModel
							{
								Hashtags = hashtags_.ToList().Select(a=>a).ToList(),
								Topic = this.Topic,
							});

							continue;
						}
						else
						{
							CommentsModel comment = new CommentsModel();
							comment.MediaId = media.Pk;
							comment.Topic = Topic;
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
				else
				{
					if (commentsOf.Info.ResponseType == ResponseType.RequestsLimit || commentsOf.Info.ResponseType == ResponseType.NetworkProblem)
					{
						return false;
					}
				}
				if (Comments.Count > 0)
				{
					_commentsRepository.AddComments(Comments);
					await _hashtagsRepository.AddHashtags(Hashtags);
					return true;
				}
				return false;
			}
			catch (Exception ee)
			{

				_reportHandler.MakeReport(ee);
				return false;
			}
		}
	}
}
