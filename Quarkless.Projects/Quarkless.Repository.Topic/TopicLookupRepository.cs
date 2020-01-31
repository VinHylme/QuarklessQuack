using MongoDB.Driver;
using Quarkless.Models.Topic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using Quarkless.Models.Topic.Interfaces;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Repository.Topic
{
	public class TopicLookupRepository : ITopicLookupRepository
	{
		private readonly IMongoCollection<CTopic> _ctx;
		private const string COLLECTION_NAME = "TopicLookup";

		public TopicLookupRepository(IMongoClientContext context)
			=> _ctx = context.ContentDatabase.GetCollection<CTopic>(COLLECTION_NAME);

		public async Task<AddTopicResponse> AddTopic(CTopic topic)
		{
			try
			{
				var response = new AddTopicResponse();
				var searchTopic = (await GetTopicsName(topic.Name)).FirstOrDefault();
				if (searchTopic != null)
				{
					response.Exists = true;
					response.Id = searchTopic._id;
					return response;
				}
				topic._id = ObjectId.GenerateNewId((int)DateTime.UtcNow.Ticks).ToString();
				await _ctx.InsertOneAsync(topic);
				response.Id = topic._id;
				return response;
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
		public async Task<List<string>> AddTopics(List<CTopic> topics)
		{
			try
			{
				await _ctx.InsertManyAsync(topics);
				return topics.Select(_ => _._id).ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<string>();
			}
		}

		public async Task<CTopic> GetTopicById(string id)
		{
			try
			{
				var filter = new FilterDefinitionBuilder<CTopic>().Eq("_id", ObjectId.Parse(id));
				var result = await _ctx.FindAsync(filter);
				return result.Single();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
		public async Task<List<CTopic>> GetTopicsByParentId(string parentId)
		{
			try
			{
				var filter = new FilterDefinitionBuilder<CTopic>().Eq("ParentTopicId", parentId);
				var result = await _ctx.FindAsync(filter);
				return result.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<CTopic>();
			}
		}

		public async Task<List<CTopic>> GetCategories()
		{
			try
			{
				var filter = new FilterDefinitionBuilder<CTopic>().Eq("ParentTopicId", BsonObjectId.Empty);
				var results = await _ctx.FindAsync(filter);
				return results.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<CTopic>();
			}
		}

		public async Task<List<CTopic>> GetAllTopics()
		{
			try
			{
				var results = await _ctx.FindAsync(_ => true);
				return results.ToList();
			}
			catch(Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<CTopic>();
			}
		}

		public async Task<List<CTopic>> GetTopicsNameLike(string name)
		{
			try
			{
				var filter = new FilterDefinitionBuilder<CTopic>().Regex("Name", new BsonRegularExpression(".*" + name + ".*"));
				var result = await _ctx.FindAsync(filter);
				return result.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<CTopic>();
			}
		}
		public async Task<List<CTopic>> GetTopicsName(string name)
		{
			try
			{
				var filter = new FilterDefinitionBuilder<CTopic>().Eq("Name", name);
				var result = await _ctx.FindAsync(filter);
				return result.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<CTopic>();
			}
		}

		public async Task<long> DeleteAll(params string[] topicsId)
		{
			try
			{
				var filter = new FilterDefinitionBuilder<CTopic>().In("_id", topicsId);
				var results = await _ctx.DeleteManyAsync(filter);
				return results.DeletedCount;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return 0;
			}
		}
		public async Task<long> DeleteAll()
		{
			try
			{
				//delete all topics expect the category topics
				var filter = new FilterDefinitionBuilder<CTopic>().Ne("ParentTopicId", BsonObjectId.Empty);
				var results = await _ctx.DeleteManyAsync(filter);
				return results.DeletedCount;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return 0;
			}
		}
	}
}
