using Quarkless.Models.Hashtag.Interfaces;
using Quarkless.Models.HashtagGenerator.Interfaces;
using Quarkless.Models.Topic;
using Quarkless.Models.Topic.Interfaces;
using Quarkless.Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;
using MoreLinq.Extensions;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.HashtagGenerator;
using Quarkless.Models.Common.Models;

namespace Quarkless.Logic.HashtagGenerator
{
	public class HashtagGenerator : IHashtagGenerator
	{
		private readonly ITopicLookupLogic _topicLookup;
		private readonly IHashtagLogic _hashtagLogic;
		private readonly IVisionClient _visionClient;
		public HashtagGenerator(IHashtagLogic hashtagLogic, IVisionClient client, ITopicLookupLogic topicLookup)
		{
			_hashtagLogic = hashtagLogic;
			_visionClient = client;
			_topicLookup = topicLookup;
		}

		public async Task<List<HashtagResponse>> GetHashtagsFromMediaSearch(string topic, int limit)
		{
			var results = new List<HashtagResponse>();
			if (string.IsNullOrEmpty(topic))
				return results;

			var response = await _topicLookup.GetMediasFromTopic(topic, limit);
			
			if (!response.Any())
				return results;

			results.AddRange(response
				.SelectMany(_ => _.Caption?.Text.FilterHashtags())
				.Where(_ => !string.IsNullOrEmpty(_) && _.Length > 3 && _.IsUsingLatinCharacters())
				.Select(_=> new HashtagResponse
				{
					Name = _.RemoveFirstHashtagCharacter(),
					IsMediaExample = true,
					Rarity = HashtagRarity.Common
				}));

			return results;
		}
		public async Task<ResultCarrier<List<HashtagResponse>>> SuggestHashtagsFromImages(
			bool deepDive = false, bool includeMediaExample = true, params string[] images)
		{
			var result = new ResultCarrier<List<HashtagResponse>>
			{
				Results = new List<HashtagResponse>()
			};

			if (!images.Any())
			{
				result.IsSuccessful = false;
				result.Info = new ErrorResponse
				{
					Message = "Image is not provided"
				};
			}

			try
			{
				var visionResults = new List<KeyWordsContainer>();
				var currentImage = 0;

				foreach (var image in images)
				{
					var web = new List<WebDetection>();
					if (image.IsBase64String())
						web.AddRange(await _visionClient.DetectImageWebEntities(images
							.Select(_ => Convert.FromBase64String(_.Split(",")[1]))));
					
					else
						web.AddRange(await _visionClient.DetectImageWebEntities(images.ToArray()));
					
					if (web.Any())
						visionResults.AddRange(web.SelectMany(_ =>
							_.WebEntities.OrderByDescending(s => s.Score)
								.Select(s => new KeyWordsContainer(s.Description, true, currentImage++))));
				}

				visionResults = visionResults
					.DistinctBy(_ => _.Keyword)
					.Shuffle()
					.Take(5)
					.ToList();

				var counter = 0;
				do
				{
					var keyword = visionResults[counter];

					var resultBuild = await _topicLookup.BuildRelatedTopics(new CTopic
					{
						Name = keyword.Keyword.String.RemoveNonWordsFromText()
					}, false, false);

					if (!resultBuild.Any()) continue;
					
					result.Results.AddRange(resultBuild);

					if (includeMediaExample)
					{
						var mediaExamples = await GetHashtagsFromMediaSearch(resultBuild.Shuffle().First().Name, 1);

						if (mediaExamples.Any())
						{
							mediaExamples.ForEach(_=>_.IsMediaDescriptor=true);
							result.Results.AddRange(mediaExamples);
						}
					}

					if (!deepDive) break;
					counter++;

				} while (visionResults.Count > counter);

				result.IsSuccessful = true;
				return result;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				result.IsSuccessful = false;
				result.Info = new ErrorResponse
				{
					Exception = err,
					Message = err.Message
				};
				return result;
			}
		}
		public async Task<ResultCarrier<List<HashtagResponse>>> SuggestHashtagsFromImages(
			List<byte[]> images, bool deepDive = false, bool includeMediaExamples = true)
		{
			var result = new ResultCarrier<List<HashtagResponse>>
			{
				Results = new List<HashtagResponse>()
			};

			if (!images.Any())
			{
				result.IsSuccessful = false;
				result.Info = new ErrorResponse
				{
					Message = "Image is not provided"
				};
			}

			try
			{
				var visionResults = new List<KeyWordsContainer>();
				var currentImage = 0;

				foreach (var image in images)
				{
					var web = new List<WebDetection>();
					web.AddRange(await _visionClient.DetectImageWebEntities(images.ToArray()));

					if (web.Any())
						visionResults.AddRange(web.SelectMany(_ =>
							_.WebEntities.OrderByDescending(s => s.Score)
								.Select(s => new KeyWordsContainer(s.Description, true, currentImage++))));
				}

				visionResults = visionResults
					.DistinctBy(_ => _.Keyword)
					.Shuffle()
					.Take(5)
					.ToList();

				var counter = 0;
				do
				{
					var keyword = visionResults[counter];

					var resultBuild = await _topicLookup.BuildRelatedTopics(new CTopic
					{
						Name = keyword.Keyword.String.RemoveNonWordsFromText()
					}, false, false);

					if (!resultBuild.Any()) continue;

					result.Results.AddRange(resultBuild);

					if (includeMediaExamples)
					{
						var mediaExamples = await GetHashtagsFromMediaSearch(resultBuild.Shuffle().First().Name, 1);

						if (mediaExamples.Any())
						{
							mediaExamples.ForEach(_ => _.IsMediaDescriptor = true);
							result.Results.AddRange(mediaExamples);
						}
					}

					if (!deepDive) break;
					counter++;

				} while (visionResults.Count > counter);

				result.IsSuccessful = true;
				return result;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				result.IsSuccessful = false;
				result.Info = new ErrorResponse
				{
					Exception = err,
					Message = err.Message
				};
				return result;
			}
		}

		public async Task<ResultCarrier<List<HashtagResponse>>> SuggestHashtags(Source source,
			bool deepDive = false, bool includeMediaExample = true)
		{
			var results = new ResultCarrier<List<HashtagResponse>>
			{
				Results = new List<HashtagResponse>()
			};

			try
			{
				if (source.ImageUrls != null && source.ImageUrls.Any())
				{
					var imageHashtagSuggestionResults =
						await SuggestHashtagsFromImages(deepDive, includeMediaExample, source.ImageUrls.ToArray());

					if(imageHashtagSuggestionResults.IsSuccessful)
						results.Results.AddRange(imageHashtagSuggestionResults.Results);
				}

				if (source.ImageBytes != null && source.ImageBytes.Any())
				{
					var imageHashtagSuggestionResults =
						await SuggestHashtagsFromImages(source.ImageBytes.ToList(), deepDive, includeMediaExample);

					if (imageHashtagSuggestionResults.IsSuccessful)
						results.Results.AddRange(imageHashtagSuggestionResults.Results);
				}

				if (source.ProfileTopic != null && source.ProfileTopic.Topics.Any())
				{
					var resultProfileTopic = await _topicLookup
						.GetTopicsByParentId(source.ProfileTopic.Topics.TakeAny(1).First()._id);

					if (resultProfileTopic.Any())
					{
						results.Results.AddRange(resultProfileTopic.Select(_=> new HashtagResponse
						{
							Name = _.Name,
							IsMediaExample = false,
							IsRecommended = true,
							Rarity = HashtagRarity.ProfileBasedSuggestion
						}));
					}
				}

				if (source.MediaTopic != null)
				{
					var resultMediaTopic = await _topicLookup.GetTopicsByParentId(source.MediaTopic._id);

					results.Results.AddRange(resultMediaTopic.Select(_ => new HashtagResponse
					{
						Name = _.Name,
						IsMediaExample = false,
						IsRecommended = true,
						Rarity = HashtagRarity.ProfileBasedSuggestion
					}));

					//build from database of hashtags
					var resDb = await _hashtagLogic.GetHashtagsFromRepositoryByTopic(source.MediaTopic.Name, 25);
					if (resDb != null && resDb.Any())
						results.Results.AddRange(resDb.SelectMany(_=>_.Hashtags).Select(_ => new HashtagResponse
						{
							IsMediaExample = true,
							Name = _.RemoveFirstHashtagCharacter(),
							Rarity = HashtagRarity.Common
						}));
					else
					{
						if (includeMediaExample)
						{
							var mediaExamples = await GetHashtagsFromMediaSearch(source.MediaTopic.Name, 1);
							if (mediaExamples.Any())
								results.Results.AddRange(mediaExamples);
						}
					}

					var hashtagSearch = await _topicLookup.BuildRelatedTopics(source.MediaTopic, false, false);
					results.Results.AddRange(hashtagSearch);
				}

				var filteredResults = results.Results
					.DistinctBy(_ => _.Name)
					.GroupBy(_=>_.Rarity)
					.ToList();

				var recommended = new List<HashtagResponse>();

				foreach (var filteredResult in filteredResults)
				{
					switch (filteredResult.Key)
					{
						case HashtagRarity.Common:
							recommended.AddRange(filteredResult
								.Where(_=>_.Name.IsUsingLatinCharacters() && _.Name.Length > 2)
								.OrderByDescending(_ => _.IsMediaDescriptor).Take(7));
							break;
						case HashtagRarity.Average:
							recommended.AddRange(filteredResult
								.Where(_ => _.Name.IsUsingLatinCharacters() && _.Name.Length > 2)
								.OrderByDescending(_ => _.IsMediaDescriptor).Take(6));
							break;
						case HashtagRarity.Rare:
							recommended.AddRange(filteredResult
								.Where(_ => _.Name.IsUsingLatinCharacters() && _.Name.Length > 2)
								.OrderByDescending(_ => _.IsMediaDescriptor).Take(3));
							break;
						case HashtagRarity.ExternalSuggestion:
							recommended.AddRange(filteredResult
								.Where(_ => _.Name.IsUsingLatinCharacters() && _.Name.Length > 2)
								.OrderByDescending(_ => _.IsMediaDescriptor).Take(3));
							break;
						case HashtagRarity.ProfileBasedSuggestion:
							recommended.AddRange(filteredResult
								.Where(_ => _.Name.IsUsingLatinCharacters() && _.Name.Length > 2)
								.OrderByDescending(_ => _.IsMediaDescriptor).Take(7));
							break;
					}
				}
				const int maxTake = 30;
				if (recommended.Count < maxTake)
				{
					recommended.AddRange(filteredResults.SelectMany(_=>_)
						.Where(_=>!recommended.Exists(s=>s.Name.Equals(_.Name)))
						.OrderByDescending(_=>_.IsRecommended)
						.Shuffle()
						.Take(maxTake - recommended.Count));
				}

				recommended.ForEach(_=> _.Name = "#" + _.Name);

				results.Results = recommended;
				results.IsSuccessful = true;
				return results;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Exception = err,
					Message = err.Message
				};
				return results;
			}
		}

		#region Old Code
		/*
		public async Task<List<string>> SuggestHashtags(Topic profileTopic = null, CTopic mediaTopic = null, 
			IEnumerable<string> images = null, int pickAmount = 20, int keywordsFetchAmount = 3, 
			IEnumerable<string> preDefinedHashtagsToUse = null, int retries = 3)
		{
			if (pickAmount < 5)
				throw new Exception("Pick amount should be greater than 8");
			if(mediaTopic == null && images == null)
				throw new Exception("Please provide at least a media topic or one image");

			#region Initialise Amount 
			var amount = (int) (pickAmount * 0.50);
			#endregion

			var suggestedHashtags = new List<string>();
			if (preDefinedHashtagsToUse != null)
				suggestedHashtags.AddRange(preDefinedHashtagsToUse);

			#region Initialise Keywords From Google Vision
			var visionResults = new List<KeyWordsContainer>();

			// By Vision API
			if (images != null && images.Any())
			{
				if (images.First().IsBase64String())
				{
					var response = _visionClient.AnnotateImages(images
							.Select(_ => Convert.FromBase64String(_.Split(",")[1])))
						.OrderByDescending(x=>x.Score)
						.Select(_ => _.Description);

					var web = await _visionClient.DetectImageWebEntities(images
						.Select(_ => Convert.FromBase64String(_.Split(",")[1])));

					if (web.Any())
					{
						visionResults.AddRange(web.SelectMany(_=>
							_.WebEntities.OrderByDescending(s=>s.Score)
								.Select(s=>new KeyWordsContainer(s.Description, true, 0))));
					}

					if (response.Any())
						visionResults.AddRange(response
							.Select(s=>new KeyWordsContainer(s,true,1)).Take(keywordsFetchAmount));
				}
				else
				{
					var response = _visionClient.AnnotateImages(images)
						.OrderByDescending(x => x.Score)
						.Select(_ => _.Description);

					var web = await _visionClient.DetectImageWebEntities(images.ToArray());

					if (web.Any())
					{
						visionResults.AddRange(web.SelectMany(_ =>
							_.WebEntities.OrderByDescending(s => s.Score)
								.Select(s => new KeyWordsContainer(s.Description, true, 0))));
					}

					if (response.Any())
						visionResults.AddRange(response
							.Select(s => new KeyWordsContainer(s, true, 1)).Take(keywordsFetchAmount));
				}
			}

			//order by the best match for media
			visionResults = visionResults.DistinctBy(x => x).ToList();
			#endregion

			var hashtags = new List<string>();
			try
			{
				if (mediaTopic != null)
				{
					suggestedHashtags.AddRange((await _topicLookup.GetTopicsByParentId(mediaTopic._id))
						.Select(_ => _.Name)
						.TakeAny(amount));

					var resDb = await _hashtagLogic.GetHashtagsFromRepositoryByTopic(mediaTopic.Name, 25);
					
					if (resDb != null && resDb.Any())
						hashtags.AddRange(resDb.SelectMany(_ => _.Hashtags));
					else
					{
						var getFromMedias = await GetHashtagsFromMediaSearch(mediaTopic.Name, 1);
						if (getFromMedias.Any())
							hashtags.AddRange(getFromMedias.Select(_=>_.Name).OrderByDescending(hashtag=>hashtag.Similarity(mediaTopic.Name)));
					}
				}

				foreach (var keyword in visionResults.Shuffle().DistinctBy(x=>x.TypeId).Take(2))
				{
					var resultBuild = await _topicLookup.BuildRelatedTopics(new CTopic
						{
							Name = keyword.Keyword.String.RemoveNonWordsFromText(),
							ParentTopicId = mediaTopic?._id
						},
						false, false);

					if (resultBuild == null || !resultBuild.Any()) continue;

					hashtags.AddRange(resultBuild.Select(_=>_.Name));

					var getFromMedias = await GetHashtagsFromMediaSearch(resultBuild.TakeAny(1).First().Name, 1);

					if (getFromMedias.Any())
						hashtags.AddRange(getFromMedias.Select(_=>_.Name).OrderByDescending(_ => _.ToLower()
							.Similarity(keyword.Keyword.String.RemoveLargeSpacesInText(1).ToLower())));
				}

				var squashHashtags = hashtags.TakeAny(pickAmount - suggestedHashtags.Count);
				suggestedHashtags.AddRange(squashHashtags);

				if (suggestedHashtags.Count > pickAmount)
					suggestedHashtags = suggestedHashtags.Take(pickAmount).ToList();

				if (profileTopic != null && profileTopic.Topics.Any())
				{
					var resultProfileTopic = await _topicLookup.GetTopicsByParentId(profileTopic.Topics.TakeAny(1).First()._id);
					if (resultProfileTopic.Any() && suggestedHashtags.Count > 5)
					{
						suggestedHashtags.RemoveRange(suggestedHashtags.Count - 5, 5);
						suggestedHashtags.AddRange(resultProfileTopic.Select(_ => _.Name).TakeAny(5));
					}
				}

				if (suggestedHashtags.Count < 10 && retries > 0)
					return await SuggestHashtags(profileTopic, mediaTopic, images,
						(pickAmount - suggestedHashtags.Count), keywordsFetchAmount,
						suggestedHashtags, --retries);

				return suggestedHashtags.Select(_ => "#" + _.RemoveFirstHashtagCharacter()).ToList();
			}
			catch(Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<string>();
			}
		}

		public async Task<List<string>> SuggestHashtags(Topic profileTopic = null, CTopic mediaTopic = null,
			IEnumerable<byte[]> images = null, int pickAmount = 20, int keywordsFetchAmount = 4,
			IEnumerable<string> preDefinedHashtagsToUse = null, int retries = 3)
		{
			if (pickAmount < 5)
				throw new Exception("Pick amount should be greater than 8");
			if (mediaTopic == null && images == null)
				throw new Exception("Please provide at least a media topic or one image");

			#region Initialise Amount 
			var amount = (int)(pickAmount * 0.50);
			#endregion

			var suggestedHashtags = new List<string>();
			if (preDefinedHashtagsToUse != null)
				suggestedHashtags.AddRange(preDefinedHashtagsToUse);

			#region Initialise Keywords From Google Vision
			var visionResults = new List<KeyWordsContainer>();
			// By Vision API
			if (images != null && images.Any())
			{

				var response = _visionClient.AnnotateImages(images)
					.OrderByDescending(x => x.Score)
					.Select(_ => _.Description);

				var web = await _visionClient.DetectImageWebEntities(images.ToArray());

				if (web.Any())
				{
					visionResults.AddRange(web.SelectMany(_ =>
						_.WebEntities.OrderByDescending(s => s.Score)
							.Select(s => new KeyWordsContainer(s.Description, true, 0))));
				}

				if (response.Any())
					visionResults.AddRange(response
						.Select(s => new KeyWordsContainer(s, true, 1)).Take(keywordsFetchAmount));
			}

			//order by the best match for media
			visionResults = visionResults.DistinctBy(x => x).ToList();
			#endregion

			var hashtags = new List<string>();
			try
			{
				if (mediaTopic != null)
				{
					// add related topics
					suggestedHashtags.AddRange((await _topicLookup.GetTopicsByParentId(mediaTopic._id))
						.Select(_ => _.Name)
						.TakeAny(amount));

					//build from database of hashtags
					var resDb = await _hashtagLogic.GetHashtagsFromRepositoryByTopic(mediaTopic.Name, 25);
					if (resDb != null && resDb.Any())
						hashtags.AddRange(resDb.SelectMany(_ => _.Hashtags));
					else
					{
						var getFromMedias = await GetHashtagsFromMediaSearch(mediaTopic.Name, 1);
						if (getFromMedias.Any())
							hashtags.AddRange(getFromMedias.Select(_=>_.Name));
					}
				}

				foreach (var keyword in visionResults.Shuffle().DistinctBy(x => x.TypeId).Take(2))
				{
					var resultBuild = await _topicLookup.BuildRelatedTopics(new CTopic
					{
						Name = keyword.Keyword.String.RemoveNonWordsFromText(),
						ParentTopicId = mediaTopic?._id
					},
						false, false);

					if (resultBuild == null || !resultBuild.Any()) continue;
					hashtags.AddRange(resultBuild.Select(_=>_.Name));

					var getFromMedias =
						await GetHashtagsFromMediaSearch(resultBuild.TakeAny(1).First().Name, 1);

					if (getFromMedias.Any())
						hashtags.AddRange(getFromMedias.Select(_=>_.Name).OrderByDescending(_ => _.ToLower()
							.Similarity(keyword.Keyword.String.RemoveLargeSpacesInText(1).ToLower())));
				}

				var squashHashtags = hashtags.TakeAny(pickAmount - suggestedHashtags.Count);
				suggestedHashtags.AddRange(squashHashtags);

				if (suggestedHashtags.Count > pickAmount)
					suggestedHashtags = suggestedHashtags.Take(pickAmount).ToList();

				if (profileTopic != null && profileTopic.Topics.Any())
				{
					var resultProfileTopic = await _topicLookup.GetTopicsByParentId(profileTopic.Topics.TakeAny(1).First()._id);
					if (resultProfileTopic.Any() && suggestedHashtags.Count > 5)
					{
						suggestedHashtags.RemoveRange(suggestedHashtags.Count - 5, 5);
						suggestedHashtags.AddRange(resultProfileTopic.Select(_ => _.Name).TakeAny(5));
					}
				}

				if (suggestedHashtags.Count < 10 && retries > 0)
					return await SuggestHashtags(profileTopic, mediaTopic, images,
						(pickAmount - suggestedHashtags.Count), keywordsFetchAmount,
						suggestedHashtags, --retries);

				return suggestedHashtags.Select(_ => "#" + _.RemoveFirstHashtagCharacter()).ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<string>();
			}
		}
		*/
		#endregion
	}
}
