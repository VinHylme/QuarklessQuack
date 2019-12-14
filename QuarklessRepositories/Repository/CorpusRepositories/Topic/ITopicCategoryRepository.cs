using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Topics;

namespace QuarklessRepositories.Repository.CorpusRepositories.Topic
{
	public interface ITopicCategoryRepository
	{
		Task AddCategories(IEnumerable<TopicCategory> topicCategories);
		Task<IEnumerable<TopicCategory>> GetAllCategories();
		Task<IEnumerable<TopicCategory>> GetCategory(string topicName);
	}
}