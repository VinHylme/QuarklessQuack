using InstagramApiSharp.Classes.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Quarkless.Base.Auth.Common.Models.Enums;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.Hashtag.Models.Interfaces;
using Quarkless.Base.InstagramDiscover;
using Quarkless.Base.ResponseResolver.Models.Interfaces;

namespace Quarkless.Controllers
{
    [ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class DiscoverController : ControllerBaseExtended
    {
		private readonly IDiscoverLogic _discoverLogic;
		private readonly IHashtagLogic _hashtagLogic;
		private readonly IUserContext _userContext;
		private readonly IResponseResolver _responseResolver;
		public DiscoverController(IUserContext userContext, IDiscoverLogic discoverLogic,
			IHashtagLogic hashtagLogic, IResponseResolver responseResolver)
		{
			_discoverLogic = discoverLogic;
			_hashtagLogic = hashtagLogic;
			_userContext = userContext;
			_responseResolver = responseResolver;
		}

		[HttpGet]
		[Route("api/discover/followingRecentActivityFeed/{limit}")]
		public async Task<IActionResult> GetFollowingRecentActivityFeed(int limit = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetFollowingRecentActivityFeed(limit));
			return ResolverResponse(results);
		}
		[HttpGet]
		[Route("api/discover/SearchTopic/{topic}/{limit}")]
		public async Task<IActionResult> SearchTopMediaByTopic(string topic, int limit = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid, empty id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _hashtagLogic.GetTopHashtagMediaListAsync(topic,limit));
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/ExplorerFeed/{limit}")]
		public async Task<IActionResult> GetExploreFeedAsync(int limit = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid, empty id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetExploreFeedAsync(limit));
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/GetRecentActivityFeed/{limit}")]
		public async Task<IActionResult> GetRecentActivityFeed(int limit = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetRecentActivityFeedAsync(limit));
			
			return ResolverResponse(results);
		}
		
		[HttpGet]
		[Route("api/discover/GetLikedFeed/{limit}")]
		public async Task<IActionResult> GetLikedFeed(int limit = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetLikedFeed(limit));
			
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/GetTagFeed/{searchTag}/{limit}")]
		public async Task<IActionResult> GetTagFeed(string searchTag = "", int limit = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetTagFeed(searchTag,limit));
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/GetUserTimelineFeed/{limit}")]
		public async Task<IActionResult> GetUserTimelineFeed(int limit = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetUserTimelineFeed(limit));
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/GetChainingUser")]
		public async Task<IActionResult> GetChainingUser()
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetChainingUser());
			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/discover/SyncContacts")]
		public async Task<IActionResult> SyncContacts(InstaContact[] contacts)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1).WithResolverAsync(()=> _discoverLogic.SyncContacts(contacts));
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/RecentSearches")]
		public async Task<IActionResult> RecentSearches()
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.RecentSearches());
			return ResolverResponse(results);
		}
		
		[HttpGet]
		[Route("api/discover/ClearRecentSearches")]
		public async Task<IActionResult> ClearRecentSearches()
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.ClearRecentSearches());
			return ResolverResponse(results);
		}
		
		[HttpGet]
		[Route("api/discover/SuggestedSearches/{discoverySearchType}")]
		public async Task<IActionResult> SuggestedSearches(int discoverySearchType = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(
					()=> _discoverLogic.SuggestedSearches((InstagramApiSharp.Enums.InstaDiscoverSearchType) discoverySearchType));
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/GetAllMediaByUsername/{searchUsername}")]
		public async Task<IActionResult> GetAllMediaByUsername(string searchUsername)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetAllMediaByUsername(searchUsername));
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/SearchUser/{username}/{limit}")]
		public async Task<IActionResult> SearchUser(string username, int limit = 1)
		{
			if (string.IsNullOrEmpty(username) || !_userContext.UserAccountExists)
				return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.SearchUser(username, limit));
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/location/recent/{locationId}/{limit}")]
		public async Task<IActionResult> GetRecentLocationFeed(long locationId, int limit = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetRecentLocationFeed(locationId, limit));
			return ResolverResponse(results);
		}
		[HttpGet]
		[Route("api/discover/topical/{limit}/{clusterId=}")]
		public async Task<IActionResult> GetTopicalExploreFeed([FromRoute]int limit, [FromRoute] string clusterId = null)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetTopicalExploreFeed(limit,clusterId));
			return ResolverResponse(results);
		}
		[HttpGet]
		[Route("api/discover/location/top/{locationId}/{limit}")]
		public async Task<IActionResult> GetTopLocationFeedsAsync(long locationId, int limit = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetTopLocationFeedsAsync(locationId, limit));
			return ResolverResponse(results);
		}
		[HttpGet]
		[Route("api/discover/location/search/{lat}/{lon}/{query}")]
		public async Task<IActionResult> SearchLocation(double lat, double lon, string query)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.SearchLocation(lat, lon, query));
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/location/places/{lat}/{lon}/{query}/{limit}")]
		public async Task<IActionResult> SearchPlaces(double lat, double lon, string query = "", int limit = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.SearchPlaces(lat, lon, query,limit));
			return ResolverResponse(results);
		}
		[HttpGet]
		[Route("api/discover/location/users/{lat}/{lon}/{username}/{limit}")]
		public async Task<IActionResult> SearchUserByLocation(double lat, double lon, string username, int limit = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.SearchUserByLocation(lat, lon, username, limit));
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/location/info/{externalIdOrFacebookPlacesId}")]
		public async Task<IActionResult> GetLocationInfoAsync(string externalIdOrFacebookPlacesId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetLocationInfoAsync(externalIdOrFacebookPlacesId));
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/discover/location/stories/{locationId}")]
		public async Task<IActionResult> GetLocationStoriesAsync(long locationId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid, empty Id");
			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _discoverLogic.GetLocationStoriesAsync(locationId));
			return ResolverResponse(results);
		}

	}
}