using System;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp;
using Quarkless.Base.Analytics.Models;
using Quarkless.Base.Analytics.Models.Enums;
using Quarkless.Base.Analytics.Models.Interfaces;
using Quarkless.Base.Topic.Models.Extensions;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.Analytics.Logic
{
	public class HashtagsAnalytics : IHashtagsAnalytics
	{
		private readonly IWorkerManager _workerManager;
		public HashtagsAnalytics(IWorkerManager workerManager)
		{
			_workerManager = workerManager;
		}

		public async Task<ResultCarrier<HashtagDetailResponse>> GetHashtagAnalysis(
			string tagName, int mediaLimit = 0)
		{
			var results = new ResultCarrier<HashtagDetailResponse>
			{
				Results = new HashtagDetailResponse()
			};

			if (string.IsNullOrEmpty(tagName))
			{
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message = "Tag Name cannot be empty"
				};
				return results;
			}

			try
			{
				await _workerManager.PerformTaskOnWorkers(async (workers) =>
				{
					await workers.PerformAction(async (worker) =>
					{
						var hashtagInfo = await worker.Client.Hashtag.GetHashtagInfoAsync(tagName);
						if (hashtagInfo.Succeeded)
						{
							results.Results.Hashtag = new HashtagObject
							{
								IsBanned = !hashtagInfo.Value.NonViolating,
								MediaCount = hashtagInfo.Value.MediaCount,
								Name = hashtagInfo.Value.Name,
								Rarity = (HashtagRarity)hashtagInfo.Value.GetRarity()
							};
						}

						var hashtagSection =
							await worker.Client.Hashtag.GetHashtagsSectionsAsync(tagName,
								PaginationParameters.MaxPagesToLoad(1));

						if (hashtagSection.Succeeded)
						{
							results.Results.TopPosts.AddRange(hashtagSection.Value.Medias.Select(_ => new HashtagMedia
							{
								ParentHashtag = tagName,
								MediaId = _.Pk,
								Caption = _.Caption?.Text,
								CommentsCount = _.CommentsCount,
								FilterType = _.FilterType,
								HasAudio = _.HasAudio,
								HasLiked = _.HasLiked,
								Height = _.Height,
								Images = _.Images,
								IsCommentingDisabled = _.IsCommentsDisabled,
								LikesCount = _.LikesCount,
								Likers = _.Likers,
								Location = _.Location,
								TakenAt = _.TakenAt,
								UnixTakenAt = _.TakenAtUnix,
								MediaType = _.MediaType,
								ViewCount = _.ViewCount,
								Videos = _.Videos,
								Width = _.Width,
								UserTags = _.UserTags,
								User = _.User,
								PostUrl = _.Url
							}));

							results.Results.RelatedHashtags
								.AddRange(hashtagSection.Value.RelatedHashtags.Select(_ => new RelatedHashtag
								{
									Id = _.Id,
									TagName = _.Name,
									Type = _.Type,
									SimilarityDistanceToParent = _.Name.Similarity(tagName)
								}));
						}

						if (mediaLimit > 0)
						{
							var topMediaResults = await worker.Client.Hashtag.GetTopHashtagMediaListAsync(tagName,
								PaginationParameters.MaxPagesToLoad(mediaLimit));
							if (topMediaResults.Succeeded)
							{
								results.Results.TopPosts.AddRange(topMediaResults.Value.Medias.Select(_ => new HashtagMedia
								{
									ParentHashtag = tagName,
									MediaId = _.Pk,
									Caption = _.Caption?.Text,
									CommentsCount = _.CommentsCount,
									FilterType = _.FilterType,
									HasAudio = _.HasAudio,
									HasLiked = _.HasLiked,
									Height = _.Height,
									Images = _.Images,
									IsCommentingDisabled = _.IsCommentsDisabled,
									LikesCount = _.LikesCount,
									Likers = _.Likers,
									Location = _.Location,
									TakenAt = _.TakenAt,
									UnixTakenAt = _.TakenAtUnix,
									MediaType = _.MediaType,
									ViewCount = _.ViewCount,
									Videos = _.Videos,
									Width = _.Width,
									UserTags = _.UserTags,
									User = _.User,
									PostUrl = _.Url
								}));
							}
						}

						var searchHashtag = await worker.Client.Hashtag.SearchHashtagAsync(tagName);

						if (searchHashtag.Succeeded)
						{
							results.Results.QueryResults.AddRange(searchHashtag.Value
								.Select(_ => new HashtagObject
								{
									IsBanned = !_.NonViolating,
									MediaCount = _.MediaCount,
									Name = _.Name,
									Rarity = (HashtagRarity)_.GetRarity(),
									SimilarityDistanceFromParent = _.Name.Similarity(tagName)
								}));
						}

						results.IsSuccessful = true;
					});
				});

				return results;
			}
			catch (Exception err)
			{
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Exception = err,
					Message = err.Message
				};
				return results;
			}
		}
	}
}
