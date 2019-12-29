﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.ServicesModels.FetcherModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.Handlers.Util;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.Logic.TopicLookupLogic;
using QuarklessLogic.ServicesLogic.CorpusLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using TimeSpan = System.TimeSpan;

namespace Quarkless.Services.DataFetcher.FetchResolver
{
	public class FetchResolver : IFetchResolver
	{
		private readonly IMediaCorpusLogic _mediaCorpusLogic;
		private readonly ICommentCorpusLogic _commentCorpusLogic;
		private readonly IHashtagLogic _hashtagCorpusLogic;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IUtilProviders _utilProviders;
		private readonly ITopicLookupLogic _topicLookupLogic;
		private readonly ConcurrentQueue<MetaDataMediaRefresh> _mediasWork;
		private readonly ConcurrentQueue<MetaDataCommentRefresh> _commentsWork;
		private int _totalWorkers = 10;
		private readonly int _updateLoopTime;
		private Timer _timer;

		public FetchResolver(IMediaCorpusLogic mediaCorpusLogic, ICommentCorpusLogic commentCorpusLogic, 
			IHashtagLogic hashtagCorpusLogic, IHeartbeatLogic heartbeatLogic, ITopicLookupLogic topicLookupLogic,
			IUtilProviders utilProviders)
		{
			_mediaCorpusLogic = mediaCorpusLogic;
			_commentCorpusLogic = commentCorpusLogic;
			_hashtagCorpusLogic = hashtagCorpusLogic;
			_heartbeatLogic = heartbeatLogic;

			_utilProviders = utilProviders;
			_topicLookupLogic = topicLookupLogic;

			_mediasWork = new ConcurrentQueue<MetaDataMediaRefresh>();
			_commentsWork = new ConcurrentQueue<MetaDataCommentRefresh>();
			_updateLoopTime = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
			_timer = SetTimer();
		}
		private Timer SetTimer()
		{
			return new Timer(async _ => await OnCallBack(), null, _updateLoopTime, Timeout.Infinite);
		}
		private async Task OnCallBack()
		{
			_timer.Dispose();

			var mediasResults = await _heartbeatLogic.GetTempMetaData<MetaDataMediaRefresh>(MetaDataTempType.Medias);

			if (mediasResults != null && mediasResults.Any())
			{
				foreach (var mediasResult in mediasResults)
				{
					AddMedias(mediasResult);
				}
				await _heartbeatLogic.DeleteMetaDataTemp(MetaDataTempType.Medias);
			}

			var commentsResults = await _heartbeatLogic.GetTempMetaData<MetaDataCommentRefresh>(MetaDataTempType.Comments);

			if (commentsResults != null && commentsResults.Any())
			{
				foreach (var commentsResult in commentsResults)
				{
					AddComments(commentsResult);
				}
				await _heartbeatLogic.DeleteMetaDataTemp(MetaDataTempType.Comments);
			}

			_timer = SetTimer();
		}

		public async Task StartService()
		{
			Console.Read();
			await OnCallBack();
			while (true)
			{
				if (_totalWorkers > 0)
				{
					_ = BeginMediaWork();
					_ = BeginCommentWork();
				}
				await Task.Delay(TimeSpan.FromSeconds(0.35));
			}
		}

		/// <summary>
		/// todo: need to relate the topics together somehow, and possibly store on a individual basis
		/// </summary>
		/// <returns></returns>
		private async Task BeginMediaWork()
		{
			if(_mediasWork.IsEmpty)
				return;

			_totalWorkers--;
			try
			{
				_mediasWork.TryDequeue(out var mediaData);
				foreach (var media in mediaData.Medias)
				{
					var hashtags = media.Caption.FilterHashtags().ToList();
					var topicTree = await _topicLookupLogic.GetHighestParents(media.Topic);
					if (!topicTree.Any())
						continue;

					var topicUniqueHash = topicTree.ComputeTopicHashCode();

					var caption = media.Caption
						.RemoveHashtagsFromText()
						.RemoveMentionsFromText()
						.RemoveCurrencyFromText()
						.RemovePhoneNumbersFromText()
						.RemoveWebAddressesFromText()
						.RemoveNonWordsFromText()
						.RemoveHorizontalSeparationFromText()
						.RemoveVerticalSeparationFromText()
						.RemoveLargeSpacesInText();

					if (!string.IsNullOrEmpty(caption) || !string.IsNullOrWhiteSpace(caption))
					{
						await Task.Delay(TimeSpan.FromSeconds(SecureRandom.NextDouble() + 1));

						var mediaCorpus = new MediaCorpus
						{
							Caption = caption,
							CommentsCount = media.CommentCount,
							LikesCount = media.LikesCount,
							Location = media.Location,
							MediaId = media.MediaId,
							TakenAt = media.TakenAt,
							OriginalCaption = media.Caption,
							ViewsCount = media.ViewCount,
							From = new From
							{
								TopicHash = topicUniqueHash,
								CategoryTopic = topicTree[0],
								TopicRequest = media.Topic,
								TopicTree = topicTree,
								InstagramAccountId = mediaData.InstagramId,
								AccountId = mediaData.AccountId
							}
						};
						Console.WriteLine("Storing post...");
						await _mediaCorpusLogic.AddMedia(mediaCorpus);
					}

					if (!hashtags.Any()) continue;
					var hashtagCorpus = new HashtagsModel
					{
						Hashtags = hashtags.ToList(),
						From = new From
						{
							TopicHash = topicUniqueHash,
							CategoryTopic = topicTree[0],
							TopicRequest = media.Topic,
							TopicTree = topicTree,
							InstagramAccountId = mediaData.InstagramId,
							AccountId = mediaData.AccountId
						}
					};
					Console.WriteLine("Storing hashtags of post...");
					await _hashtagCorpusLogic.AddHashtagsToRepository(new[] {hashtagCorpus});
				}
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
			}
			finally
			{
				_totalWorkers++;
			}
		}

		private async Task BeginCommentWork()
		{
			if (_commentsWork.IsEmpty)
				return;

			_totalWorkers--;
			try
			{
				_commentsWork.TryDequeue(out var commentData);

				var comments = new List<CommentCorpus>();

				void AddInnerComments(IEnumerable<UserResponse<InstaComment>> commentsIn)
				{
					foreach (var comment in commentsIn)
					{
						var topicTree = _topicLookupLogic.GetHighestParents(comment.Topic).Result;
						if (!topicTree.Any())
							continue;

						var topicUniqueHash = topicTree.ComputeTopicHashCode();

						var commentTags = comment.Object.Text.FilterHashtags().ToList();
						if (commentTags.Count > 3)
						{
							_hashtagCorpusLogic.AddHashtagsToRepository(new[]
							{
								new HashtagsModel
								{
									Hashtags = commentTags,
									From = new From
									{
										TopicHash = topicUniqueHash,
										CategoryTopic = topicTree[0],
										TopicRequest = comment.Topic,
										TopicTree = topicTree,
										InstagramAccountId = commentData.InstagramId,
										AccountId = commentData.AccountId
									}
								}
							});
						}

						var commentExtract = comment.Object.Text
							.RemoveHashtagsFromText()
							.RemoveMentionsFromText()
							.RemoveCurrencyFromText()
							.RemovePhoneNumbersFromText()
							.RemoveWebAddressesFromText()
							.RemoveNonWordsFromText()
							.RemoveHorizontalSeparationFromText()
							.RemoveVerticalSeparationFromText()
							.RemoveLargeSpacesInText();

						if (!string.IsNullOrEmpty(commentExtract) || !string.IsNullOrWhiteSpace(commentExtract))
							comments.Add(new CommentCorpus
							{
								Comment = commentExtract,
								Created = comment.Object.CreatedAtUtc,
								MediaId = comment.MediaId,
								NumberOfLikes = comment.Object.LikesCount,
								NumberOfReplies = comment.Object.ChildCommentCount,
								Username = comment.Username,
								IsReply = comment.Object.ParentCommentId > 0,
								From = new From
								{
									TopicHash = topicUniqueHash,
									CategoryTopic = topicTree[0],
									TopicRequest = comment.Topic,
									TopicTree = topicTree,
									InstagramAccountId = commentData.InstagramId,
									AccountId = commentData.AccountId
								}
							});

						if (comment.Object.ChildComments.Count > 0)
							AddInnerComments(comment.Object.ChildComments.Select(c => new UserResponse<InstaComment>()
							{
								UserId = c.User.Pk,
								Username = c.User.UserName,
								FullName = c.User.FullName,
								IsPrivate = c.User.IsPrivate,
								IsVerified = c.User.IsVerified,
								MediaId = comment.MediaId,
								Object = c,
								ProfilePicture = c.User.ProfilePicture,
								Topic = comment.Topic
							}));
					}
				}

				AddInnerComments(commentData.Comments);

				if (comments.Any())
				{
					Console.WriteLine("Adding Comments fetched {0}", comments.Count);
					await _commentCorpusLogic.AddComments(comments);
				}
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				
			}
			finally
			{
				_totalWorkers++;
			}
		}

		public void AddMedias(MetaDataMediaRefresh medias)
		{
			if (medias.Medias.Count > 0)
			{
				_mediasWork.Enqueue(medias);
			}
		}
		public void AddComments(MetaDataCommentRefresh comments)
		{
			if (comments.Comments.Count > 0)
			{
				_commentsWork.Enqueue(comments);
			}
		}
	}
}
