using System;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.ReportHandler.Interfaces;

namespace Quarkless.Base.InstagramDiscover
{
	public class DiscoverLogic : IDiscoverLogic
	{
		private IReportHandler _reportHandler {get; set;}
		private readonly IApiClientContainer Client;

		public DiscoverLogic(IApiClientContainer client, IReportHandler reportHandler)
		{			
			Client = client;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("Logic/Discovery");

		}

		#region Feed
		public async Task<IResult<InstaActivityFeed>> GetFollowingRecentActivityFeed(int limit)
		{
			try
			{
				var results = await Client.Feeds.GetFollowingRecentActivityFeedAsync(PaginationParameters.MaxPagesToLoad(limit));
				return results;
			}
			catch(Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaExploreFeed>> GetExploreFeedAsync(int limit)
		{
			try
			{
				return await Client.Feeds.GetExploreFeedAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch(Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaActivityFeed>> GetRecentActivityFeedAsync(int limit)
		{
			try { 

				return await Client.Feeds.GetRecentActivityFeedAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch(Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaMediaList>> GetLikedFeed(int limit)
		{
			try
			{
				return await Client.Feeds.GetLikedFeedAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaFeed>> GetTagFeed(string tagToSearch, int limit)
		{
			try
			{
				return await Client.Feeds.GetTagFeedAsync(tagToSearch, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		
		public async Task<IResult<InstaTopicalExploreFeed>> GetTopicalExploreFeed(int limit, string clusterId)
		{
			try
			{
				return await Client.Feeds.GetTopicalExploreFeedAsync(PaginationParameters.MaxPagesToLoad(limit),clusterId);
			}
			catch(Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<InstaFeed>> GetUserTimelineFeed(int limit)
		{
			try
			{
				return await Client.Feeds.GetUserTimelineFeedAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		#endregion
		public async Task<IResult<InstaSectionMedia>> GetTopHashtagMediaList(string tagname, int limit)
		{
			try
			{
				return await Client.Hashtag.GetTopHashtagMediaListAsync(tagname,PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}

		#region Discovery
		public async Task<IResult<InstaDiscoverTopSearches>> GetTopSearchResults(string query, int instaDiscoverSearchType, int timezoneoffset)
		{
			try
			{
				return await Client.Discover.GetTopSearchesAsync(query,(InstaDiscoverSearchType)instaDiscoverSearchType,timezoneoffset);
			}
			catch(Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaUserChainingList>> GetChainingUser()
		{
			try
			{
				return await Client.Discover.GetChainingUsersAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaContactUserList>> SyncContacts(params InstaContact[] contacts)
		{
			try
			{
				return await Client.Discover.SyncContactsAsync(contacts);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaDiscoverRecentSearches>> RecentSearches()
		{
			try
			{
				return await Client.Discover.GetRecentSearchesAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> ClearRecentSearches()
		{
			try
			{
				return await Client.Discover.ClearRecentSearchsAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaDiscoverSuggestedSearches>> SuggestedSearches(InstaDiscoverSearchType instaDiscoverSearchType)
		{
			try
			{
				return await Client.Discover.GetSuggestedSearchesAsync(instaDiscoverSearchType);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<string>> GetAllMediaByUsername(string username)
		{
			try { 
				var result = await Client.Media.GetMediaIdFromUrlAsync(new Uri(@"https://www.instagram.com/" + username + "/"));
				return result; 
			}
			catch(Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaDiscoverSearchResult>> SearchUser(string search, int count)
		{
			try
			{
				return await Client.Discover.SearchPeopleAsync(search, PaginationParameters.MaxPagesToLoad(count));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		#endregion

		#region Location
		public async Task<IResult<InstaSectionMedia>> GetRecentLocationFeed(long locationid, int limit)
		{
			try { 
				return await Client.Location.GetRecentLocationFeedsAsync(locationid, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch(Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaLocationShortList>> SearchLocation(double lat, double lon, string query)
		{
			try { 
				return await Client.Location.SearchLocationAsync(lat, lon, query);
			}
			catch(Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaPlaceList>> SearchPlaces(double lat, double lon, string query = "", int limit = 1)
		{
			try { 
				return await Client.Location.SearchPlacesAsync(lat, lon, query, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch(Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaUserSearchLocation>> SearchUserByLocation(double lat, double lon, string username, int limit = 1)
		{
			try { 
				return await Client.Location.SearchUserByLocationAsync(lat, lon, username, limit);
			}
			catch(Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaPlaceShort>> GetLocationInfoAsync(string externalIdOrFacebookPlacesId)
		{
			try
			{
				return await Client.Location.GetLocationInfoAsync(externalIdOrFacebookPlacesId);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaStory>> GetLocationStoriesAsync(long locationId)
		{
			try
			{
				return await Client.Location.GetLocationStoriesAsync(locationId);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaSectionMedia>> GetTopLocationFeedsAsync(long locationId, int limit=1)
		{
			try
			{
				return await Client.Location.GetTopLocationFeedsAsync(locationId, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee);
				return null;
			}
		}
		#endregion
	}
}
