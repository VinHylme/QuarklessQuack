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
		public async Task AddCategories(IEnumerable<TopicCategories> topicCategories) => await _context.TopicCategories.InsertManyAsync(topicCategories);
		public async Task<IEnumerable<TopicCategories>> GetAllCategories(){
			try { 
				return (await _context.TopicCategories.FindAsync(Builders<TopicCategories>.Filter.Empty)).ToList();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}
		public async Task<IEnumerable<TopicCategories>> GetCategory(string topicName) => (await _context.TopicCategories.FindAsync(Builders<TopicCategories>.Filter.Eq(_ => _.CategoryName, topicName))).ToList();

	}
}
