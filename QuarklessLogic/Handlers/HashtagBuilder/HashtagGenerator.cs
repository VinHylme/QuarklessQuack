using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MoreLinq;
using Quarkless.Vision;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessLogic.Logic.TopicLookupLogic;

namespace QuarklessLogic.Handlers.HashtagBuilder
{
	public class HashtagGenerator : IHashtagGenerator
	{
		private readonly ITopicLookupLogic _topicLookup;
		private readonly IHashtagLogic _hashtagLogic;
		private readonly IVisionClient _visionClient;
		public HashtagGenerator(IHashtagLogic hashtagLogic,
			IVisionClient client, ITopicLookupLogic topicLookup)
		{
			_hashtagLogic = hashtagLogic;
			_visionClient = client;
			_topicLookup = topicLookup;
		}

		public async Task<List<string>> SuggestHashtags(Topic profileTopic, CTopic mediaTopic = null, 
			int pickAmount = 20, IEnumerable<string> imageUrls = null)
		{
			if(pickAmount < 5)
				throw new Exception("Pick amount should be greater than 8");

			const int hashtagSearchLimit = 10;
			var hashtagPickAmount = pickAmount / 2;

			var suggestedHashtags = new List<string>();
			var keywords = new List<KeyWordsContainer>();

			#region Initialise Keywords

			// By Vision API
			if (imageUrls != null && imageUrls.Any())
			{
				if (imageUrls.First().IsBase64String())
				{
					var response = _visionClient.AnnotateImages(imageUrls
							.Select(_=>Convert.FromBase64String(_.Split(",")[1])))
						?.SelectMany(image => image)
						?.OrderByDescending(c => c.Description)
						?.Select(_ => new KeyWordsContainer(_.Description, true));
					if (response != null && response.Any())
						keywords.AddRange(response);
				}
				else
				{
					var response = _visionClient.AnnotateImages(imageUrls)
						?.SelectMany(image => image)
						?.OrderByDescending(c => c.Description)
						?.Select(_ => new KeyWordsContainer(_.Description, true));
					if (response != null && response.Any())
						keywords.AddRange(response);
				}
			}

			// By Media Topic
			if(mediaTopic!=null)
				keywords.Add(new KeyWordsContainer(mediaTopic.Name, true));
			
			// By Profile Category Name
			keywords.Add(new KeyWordsContainer(profileTopic.Category.Name, false));

			// By Subtopics
			keywords.AddRange(profileTopic.Topics
				.Select(_ => new KeyWordsContainer(_.Name, false)));

			//order by the best match for media
			keywords = keywords.OrderByDescending(_ => _.IsLikely)
				.DistinctBy(x=>x.Keyword)
				.ToList();
			#endregion

			var positionCurrent = 0;
			
			//todo: some modifications, if not found query instagram (possibly using a worker)
			//todo: analyse first image with that tag if any found => store hashtags
			//todo: store the keyword in the lookup table for future analysis and building of hashtags
			//todo: e.g; keyword => search insta => get random media => get hashtag and send
			//Take some from hashtag table
			while (suggestedHashtags.Count <= hashtagPickAmount)
			{
				if (keywords.Count < positionCurrent)
					break;

				var keyword = keywords[positionCurrent++];
				var hashtags = await _hashtagLogic.GetHashtagsFromRepositoryByTopic(keyword.Keyword, hashtagSearchLimit);
				if (!hashtags.Any()) continue;
				var pickHashtag = hashtags.OrderByDescending(x => x.Topic.Similarity(keyword.Keyword)).First();
				suggestedHashtags.AddRange(pickHashtag.Hashtags.TakeAny(hashtagPickAmount));
			}

			positionCurrent = 0;

			//Take some from topic lookup table
			while (suggestedHashtags.Count <= hashtagPickAmount)
			{
				if (keywords.Count < positionCurrent)
					break;

				var keyword = keywords[positionCurrent++];
				var topicsRelated = (await _topicLookup.GetTopicByNameLike(keyword.Keyword))
					.Where(t => t.ParentTopicId != BsonObjectId.Empty.AsObjectId.ToString())
					.DistinctBy(c=>c.Name);
				if(topicsRelated.Any())
					suggestedHashtags.AddRange(topicsRelated
						.Select(_=> "#" + _.Name.ToLower()));
				else
				{
					string parentId;

					if (mediaTopic!=null && keyword.Keyword.String == mediaTopic.Name.ToLower())
						parentId = mediaTopic.ParentTopicId;
					else if (keyword.Keyword.String == profileTopic.Category.Name.ToLower())
						parentId = profileTopic.Category.ParentTopicId;
					else if (profileTopic.Topics.Any(x => x.Name.ToLower() == keyword.Keyword.String))
						parentId = profileTopic.Topics.Single(x => x.Name.ToLower() == keyword.Keyword.String)
							.ParentTopicId;
					else
						parentId = profileTopic.Category.ParentTopicId;
					
					await _topicLookup.AddTopic(new CTopic
					{
						ParentTopicId = parentId,
						Name = keyword.Keyword
					});
				}
			}

			if (suggestedHashtags.Count < 8)
			{
				var topicFromLookup = await _topicLookup.GetTopicByParentId(profileTopic.Category.ParentTopicId);
				if(topicFromLookup.Any())
					suggestedHashtags.AddRange(topicFromLookup.Select(c=> "#" + c.Name.ToLower()));
			}

			if (suggestedHashtags.Count > pickAmount)
			{
				suggestedHashtags = suggestedHashtags.Take(pickAmount).ToList();
			}

			return suggestedHashtags;
		}

//		public async Task<IEnumerable<string>> BuildHashtags(string topic, string subcategory, 
//			string language = null, int limit = 1, int pickRate = 20)
//		{
//			var hashtagResults = (await _hashtagLogic.GetHashtagsByTopicAndLanguage(topic.OnlyWords(),
//				language?.ToUpper().OnlyWords(), 
//				language?.MapLanguages().OnlyWords(), limit))
//				.Shuffle().ToList();
//			
//			var clean = new Regex(@"[^\w\d]");
//			if (hashtagResults.Count <= 0) return null;
//			var hashtags = new List<string>();
//			while (hashtags.Count < pickRate)
//			{
//				var chosenHashtags = new List<string>();
//
//				foreach (var hashtagResult in hashtagResults)
//				{
//					if (string.IsNullOrEmpty(hashtagResult.Language)) continue;
//					var hashtagLanguage = clean.Replace(hashtagResult.Language.ToLower(), "");
//					var languageSelected = clean.Replace(language.MapLanguages().ToLower(), "");
//
//					if (hashtagLanguage == languageSelected)
//						chosenHashtags.AddRange(hashtagResult.Hashtags);
//				}
//
//				if (chosenHashtags.Count <= 0) continue;
//				var chosenHashtagsFiltered = chosenHashtags.Where(space => space.Count(oc => oc == ' ') <= 1);
//				var hashtagsFiltered = chosenHashtagsFiltered as string[] ?? chosenHashtagsFiltered.ToArray();
//				if (!hashtagsFiltered.Any()) return null;
//				hashtags.AddRange(hashtagsFiltered.Where(s => s.Length >= 3 && s.Length <= 30));
//			}
//			return hashtags;
//		}

	}
}
