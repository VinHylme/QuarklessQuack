using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Topics;

namespace QuarklessRepositories.Repository.CorpusRepositories.Topic
{
	public interface ITopicCategoryRepository
	{
		Task AddCategories(IEnumerable<TopicCategories> topicCategories);
		Task<IEnumerable<TopicCategories>> GetAllCategories();
		Task<IEnumerable<TopicCategories>> GetCategory(string topicName);
	}
}