using Quarkless.Models.Hashtag.Interfaces;
using Quarkless.Models.HashtagGenerator.Interfaces;
using Quarkless.Models.Profile;
using Quarkless.Models.Topic;
using Quarkless.Models.Topic.Interfaces;
using Quarkless.Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using Quarkless.Models.Common.Extensions;

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

		public async Task<List<string>> GetHashtagsFromMediaSearch(string topic, int limit)
		{
			var results = new List<string>();
			if (string.IsNullOrEmpty(topic))
				return results;
			var response = await _topicLookup.GetMediasFromTopic(topic, limit);
			
			if (!response.Any())
				return results;
			
			results.AddRange(response.SelectMany(_=>_.Caption?.Text.FilterHashtags()));

			return results.Where(_=>!string.IsNullOrEmpty(_) 
				&& _.Length > 2).ToList();
		}

		public async Task<List<string>> SuggestHashtags(Topic profileTopic = null, CTopic mediaTopic = null, IEnumerable<string> images = null,
			int pickAmount = 20, int keywordsFetchAmount = 4, 
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
			var visionResults = new List<string>();
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

					if(web.Any())
						visionResults.AddRange(web
							.SelectMany(_=>_.WebEntities)
							.OrderByDescending(_=>_.Score).Select(_=>_.Description).Take(keywordsFetchAmount));

					if (response.Any())
						visionResults.AddRange(response.Take(keywordsFetchAmount));
				}
				else
				{
					var response = _visionClient.AnnotateImages(images)
						.OrderByDescending(x => x.Score)
						.Select(_ => _.Description);

					var web = await _visionClient.DetectImageWebEntities(images.ToArray());

					if (web.Any())
						visionResults.AddRange(web
							.SelectMany(_ => _.WebEntities)
							.OrderByDescending(_ => _.Score).Select(_ => _.Description).Take(keywordsFetchAmount));

					if (response.Any())
						visionResults.AddRange(response.Take(keywordsFetchAmount));
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
							hashtags.AddRange(getFromMedias);
					}
				}

				foreach (var keyword in visionResults.TakeAny(1))
				{
					var resultBuild = await _topicLookup.BuildRelatedTopics(new CTopic
						{
							Name = keyword.RemoveNonWordsFromText(),
							ParentTopicId = mediaTopic?._id
						},
						false, false);

					if (resultBuild == null || !resultBuild.Any()) continue;
					hashtags.AddRange(resultBuild);

					var getFromMedias =
						await GetHashtagsFromMediaSearch(resultBuild.TakeAny(1).First(), 1);

					if (getFromMedias.Any())
						hashtags.AddRange(getFromMedias.OrderByDescending(_ => _.ToLower()
							.Similarity(keyword.RemoveLargeSpacesInText(1).ToLower())));
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
			var visionResults = new List<string>();
			// By Vision API
			if (images != null && images.Any())
			{

				var response = _visionClient.AnnotateImages(images)
					.OrderByDescending(x => x.Score)
					.Select(_ => _.Description);

				var web = await _visionClient.DetectImageWebEntities(images.ToArray());

				if (web.Any())
					visionResults.AddRange(web
						.SelectMany(_ => _.WebEntities)
						.OrderByDescending(_ => _.Score).Select(_ => _.Description).Take(keywordsFetchAmount));

				if (response.Any())
					visionResults.AddRange(response.Take(keywordsFetchAmount));
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
							hashtags.AddRange(getFromMedias);
					}
				}

				foreach (var keyword in visionResults.TakeAny(1))
				{
					var resultBuild = await _topicLookup.BuildRelatedTopics(new CTopic
					{
						Name = keyword.RemoveNonWordsFromText(),
						ParentTopicId = mediaTopic?._id
					},
						false, false);

					if (resultBuild == null || !resultBuild.Any()) continue;
					hashtags.AddRange(resultBuild);

					var getFromMedias =
						await GetHashtagsFromMediaSearch(resultBuild.TakeAny(1).First(), 1);

					if (getFromMedias.Any())
						hashtags.AddRange(getFromMedias.OrderByDescending(_ => _.ToLower()
							.Similarity(keyword.RemoveLargeSpacesInText(1).ToLower())));
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

	}
}
