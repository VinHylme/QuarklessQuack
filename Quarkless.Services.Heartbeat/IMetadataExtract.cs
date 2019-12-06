using System.Threading.Tasks;

namespace Quarkless.Services.Heartbeat
{
	public interface IMetadataExtract
	{
		void Initialise(Assignment assignment);
		Task BuildBase(int limit = 2, int cutBy = 1, int takeTopicAmount = 1);
		Task BuildUsersTargetListMedia(int limit = 1, int cutBy = 1);
		Task BuildLocationTargetListMedia(int limit = 1, int cutBy = 1);
		Task BuildUsersOwnMedias(int limit = 1, int cutBy = 1);
		Task BuildUsersFeed(int limit = 1, int cutBy = 1);
		Task BuildUserFollowList(int limit = 3, int cutBy = 1);
		Task BuildUserFollowerList(int limit = 4, int cutBy = 1);
		Task BuildUsersFollowSuggestions(int limit = 1, int cutObjectBy = 1);
	}
}