using MongoDB.Driver;
using QuarklessContexts.Models.Topics;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessRepositories.Repository.CorpusRepositories.Topic
{
	public class TopicCategoryRepository : ITopicCategoryRepository
	{
		private readonly IRepositoryContext _context;
		public TopicCategoryRepository(IRepositoryContext context) => _context = context;
		public async Task AddCategories(IEnumerable<TopicCategory> topicCategories) => await _context.TopicCategories.InsertManyAsync(topicCategories);
		public async Task<IEnumerable<TopicCategory>> GetAllCategories(){
			try { 
				return (await _context.TopicCategories.FindAsync(Builders<TopicCategory>.Filter.Empty)).ToList();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}
		public async Task<IEnumerable<TopicCategory>> GetCategory(string topicName) => (await _context.TopicCategories.FindAsync(Builders<TopicCategory>.Filter.Eq(_ => _.Category.CategoryName, topicName))).ToList();

	}
}
