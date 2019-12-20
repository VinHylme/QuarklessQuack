using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using QuarklessContexts.Models.Topics;
using QuarklessRepositories.RepositoryClientManager;

namespace QuarklessRepositories.Repository.TopicLookupRepository
{
	public class TopicLookupRepository : ITopicLookupRepository
	{
		private readonly IRepositoryContext _context;
		public TopicLookupRepository(IRepositoryContext context) => _context = context;

		public async Task<string> AddTopic(CTopic topic)
		{
			try
			{
				var searchTopic = (await GetTopicsName(topic.Name)).FirstOrDefault();
				if (searchTopic != null)
					return searchTopic._id;

				topic._id = ObjectId.GenerateNewId((int) DateTime.UtcNow.Ticks).ToString();
				await _context.TopicLookup.InsertOneAsync(topic);
				return topic._id;
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
				await _context.TopicLookup.InsertManyAsync(topics);
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
				var filter = new FilterDefinitionBuilder<CTopic>().Eq("Id",ObjectId.Parse(id));
				var result = await _context.TopicLookup.FindAsync(filter);
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
				var filter = new FilterDefinitionBuilder<CTopic>().Eq("ParentTopicId", ObjectId.Parse(parentId));
				var result = await _context.TopicLookup.FindAsync(filter);
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
				var results = await _context.TopicLookup.FindAsync(filter);
				return results.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<CTopic>();
			}
		}
		public async Task<List<CTopic>> GetTopicsNameLike(string name)
		{
			try
			{
				var filter = new FilterDefinitionBuilder<CTopic>().Regex("Name", new BsonRegularExpression(".*"+name+".*"));
				var result = await _context.TopicLookup.FindAsync(filter);
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
				var filter = new FilterDefinitionBuilder<CTopic>().Eq("Name",name);
				var result = await _context.TopicLookup.FindAsync(filter);
				return result.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<CTopic>();
			}
		}

		public async Task<long> DeleteAll()
		{
			try
			{
				var results = await _context.TopicLookup.DeleteManyAsync(_ => true);
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
