using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire.Mongo.Dto;

namespace QuarklessRepositories.Repository.TimelineRepository
{
	public interface ITimelineRepository
	{
		Task<IEnumerable<object>> GetAllEvents();
	}
}