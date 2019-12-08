using System.Threading.Tasks;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;

namespace Quarkless.Services.Heartbeat
{
	public interface IMetadataExtracte
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
		Task BuildUsersInbox(int limit = 1);
		Task BuildUsersRecentComments(int howManyMedias = 10, int limit = 1);
		Task BuildUserFromLikers(int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 100);
		Task BuildMediaFromUsersLikers(int limit = 1, int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 10);
		Task BuildUsersFromCommenters(int limit = 1, int cutBy = 1, double sleepTime = 0.05,
			int takeMediaAmount = 7, int takeUserMediaAmount = 100);
		Task BuildCommentsFromSpecifiedSource(MetaDataType extractFrom, MetaDataType saveTo,
			bool includeUser = false, int limit = 1, int cutBy = 1, double secondSleep = 0.05,
			int takeMediaAmount = 10, int takeuserAmount = 100);
		Task BuildMediaFromUsersCommenters(int limit = 1, int cutBy = 1, double secondsSleep = 0.05,
			int takeMediaAmount = 10, int takeUserMediaAmount = 10);

		Task BuildGoogleImages(int limit = 50, int topicAmount = 1, int cutBy = 1);
		Task BuildYandexImagesQuery(int limit = 2, int topicAmount = 1, int cutBy = 1);
		Task BuildYandexImages(int limit = 3, int takeTopicAmount = 1, int cutBy = 1);
	}
}