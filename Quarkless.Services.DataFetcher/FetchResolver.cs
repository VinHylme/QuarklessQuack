using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.ServicesModels.FetcherModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.EventHandlers;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.ServicesLogic.CorpusLogic;
using TimeSpan = System.TimeSpan;

namespace Quarkless.Services.DataFetcher
{
	public class FetchResolver :IFetchResolver , IEventSubscriberSync<MetaDataMediaRefresh>, IEventSubscriberSync<MetaDataCommentRefresh>
	{
		private readonly IMediaCorpusLogic _mediaCorpusLogic;
		private readonly ICommentCorpusLogic _commentCorpusLogic;
		private readonly IHashtagLogic _hashtagCorpusLogic;
		private readonly ConcurrentQueue<MetaDataMediaRefresh> _mediasWork;
		private readonly ConcurrentQueue<MetaDataCommentRefresh> _commentsWork;
		private int _totalWorkers = 10;
		public FetchResolver(IMediaCorpusLogic mediaCorpusLogic, ICommentCorpusLogic commentCorpusLogic, 
			IHashtagLogic hashtagCorpusLogic)
		{
			_mediaCorpusLogic = mediaCorpusLogic;
			_commentCorpusLogic = commentCorpusLogic;
			_hashtagCorpusLogic = hashtagCorpusLogic;

			_mediasWork = new ConcurrentQueue<MetaDataMediaRefresh>();
			_commentsWork = new ConcurrentQueue<MetaDataCommentRefresh>();
		}

		public async Task StartService()
		{
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

		private async Task BeginMediaWork()
		{
			if(_mediasWork.IsEmpty)
				return;

			_totalWorkers--;

			_mediasWork.TryDequeue(out var mediaData);
			foreach (var media in mediaData.Medias)
			{
				var hashtags = media.Caption.FilterHashtags().ToList();
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
						Topic = media.Topic.Name,
						ViewsCount = media.ViewCount,
					};
					Console.WriteLine("Storing post...");
					await _mediaCorpusLogic.AddMedia(mediaCorpus);
				}

				if (!hashtags.Any()) continue;
				var hashtagCorpus = new HashtagsModel
				{
					Hashtags = hashtags.ToList(),
					Topic = media.Topic.Name
				};
				Console.WriteLine("Storing hashtags of post...");
				await _hashtagCorpusLogic.AddHashtagsToRepository(new[] { hashtagCorpus });
			}

			_totalWorkers++;
		}

		private async Task BeginCommentWork()
		{
			if (_commentsWork.IsEmpty)
				return;

			_totalWorkers--;
			_commentsWork.TryDequeue(out var commentData);

			var comments = new List<CommentCorpus>();

			void AddInnerComments(IEnumerable<UserResponse<InstaComment>> commentsIn)
			{
				foreach (var comment in commentsIn)
				{
					var commentTags = comment.Object.Text.FilterHashtags().ToList();
					if (commentTags.Count > 3)
					{
						_hashtagCorpusLogic.AddHashtagsToRepository(new[]
						{
							new HashtagsModel
							{
								Hashtags = commentTags,
								Topic = comment.Topic.Name
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
							Topic = comment.Topic.Name,
							Username = comment.Username,
							IsReply = comment.Object.ParentCommentId > 0
						});

					if (comment.Object.ChildComments.Count > 0)
						AddInnerComments(comment.Object.ChildComments.Select(c=> new UserResponse<InstaComment>()
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

			_totalWorkers++;
		}

		public void Handle(MetaDataMediaRefresh @event)
		{
			if (@event.Medias.Count > 0)
			{
				_mediasWork.Enqueue(@event);
			}
		}
		public void Handle(MetaDataCommentRefresh @event)
		{
			if (@event.Comments.Count > 0)
			{
				_commentsWork.Enqueue(@event);
			}
		}
	}
}
