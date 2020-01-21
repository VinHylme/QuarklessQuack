using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using System.Threading.Tasks;

namespace Quarkless.Base.InstagramDiscover
{
	public interface IDiscoverLogic
	{
		Task<IResult<InstaSectionMedia>> GetTopHashtagMediaList(string tagname, int limit);
		Task<IResult<InstaDiscoverTopSearches>> GetTopSearchResults(string query, int instaDiscoverSearchType, int timezoneoffset);
		Task<IResult<InstaActivityFeed>> GetFollowingRecentActivityFeed(int limit);
	    Task<IResult<InstaExploreFeed>> GetExploreFeedAsync(int limit);
		Task<IResult<InstaActivityFeed>> GetRecentActivityFeedAsync(int limit);
		Task<IResult<InstaMediaList>> GetLikedFeed(int limit);
		Task<IResult<InstaFeed>> GetTagFeed(string tagToSearch, int limit);
		Task<IResult<InstaFeed>> GetUserTimelineFeed(int limit);
		Task<IResult<InstaUserChainingList>> GetChainingUser();
		Task<IResult<InstaContactUserList>> SyncContacts(params InstaContact[] contacts);
		Task<IResult<InstaDiscoverRecentSearches>> RecentSearches();
		Task<IResult<bool>> ClearRecentSearches();
		Task<IResult<InstaDiscoverSuggestedSearches>> SuggestedSearches(InstaDiscoverSearchType instaDiscoverSearchType);
		Task<InstagramApiSharp.Classes.IResult<string>> GetAllMediaByUsername(string username);
		Task<IResult<InstaDiscoverSearchResult>> SearchUser(string name, int limit);
		Task<IResult<InstaSectionMedia>> GetRecentLocationFeed(long locationid, int limit);
		Task<IResult<InstaLocationShortList>> SearchLocation(double lat, double lon, string query);
		Task<IResult<InstaPlaceList>> SearchPlaces(double lat, double lon, string query = "", int limit = 1);
		Task<IResult<InstaUserSearchLocation>> SearchUserByLocation(double lat, double lon, string username, int limit = 1);
		Task<IResult<InstaPlaceShort>> GetLocationInfoAsync(string externalIdOrFacebookPlacesId);
		Task<IResult<InstaStory>> GetLocationStoriesAsync(long locationId);
		Task<IResult<InstaSectionMedia>> GetTopLocationFeedsAsync(long locationId, int limit = 1);
		Task<IResult<InstaTopicalExploreFeed>> GetTopicalExploreFeed(int limit, string clusterId);
	}
}