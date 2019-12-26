using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.QueryLogic;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.QueryModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class QueryController : ControllerBase
	{
		private readonly IQueryLogic _queryLogic; 
		private readonly IUserContext _userContext;
		public QueryController(IQueryLogic queryLogic, IUserContext userContext)
		{
			_queryLogic = queryLogic;
			_userContext = userContext;
		}

		[HttpGet]
		[Route("api/query/config")]
		public async Task<IActionResult> GetProfileConfig()
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetProfileConfig());
		}

		[HttpGet]
		[Route("api/query/relatedtopics/{topicParentId}")]
		public async Task<IActionResult> GetRelatedTopics(string topicParentId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetRelatedTopics(topicParentId));
		}
		[HttpGet]
		[Route("api/query/releated/{topic}")]
		public async Task<IActionResult> SearchReleatedTopic(string topic)
		{
			if (!_userContext.UserAccountExists && string.IsNullOrEmpty(topic))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetRelatedKeywords(topic));
		}

		[HttpPost]
		[Route("api/query/build/tags")]
		public async Task<IActionResult> BuildTags(SuggestHashtagRequest suggestHashtagRequest)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.BuildHashtags(suggestHashtagRequest));
		}
		[HttpGet]
		[Route("api/query/search/places/{query}")]
		public IActionResult SearchPlaces(string query)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(_queryLogic.SearchPlaces(query));
		}

		[HttpGet]
		[Route("api/query/search/places/auto/{query}/{radius}")]
		public IActionResult AutoCompleteSearchPlaces(string query, int radius = 500)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(_queryLogic.AutoCompleteSearchPlaces(query,radius));
		}

		[HttpPut]
		[Route("api/query/search/similar/{limit}/{offset=0}/{moreAccurate}")]
		public async Task<IActionResult> SimilarSearch([FromBody] IEnumerable<string> urls, [FromRoute] int limit, [FromRoute] int offset = 0, [FromRoute] bool moreAccurate = false)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.SimilarImagesSearch(_userContext.CurrentUser, limit, offset, urls,moreAccurate));
		}

		[HttpPost]
		[Route("api/query/searchMediaTopic/{instagramId}/{limit}")]
		public async Task<IActionResult> SearchMediasByTopic([FromBody] UserSearchRequest queryObject, string instagramId, int limit)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser) || string.IsNullOrEmpty(queryObject.Query))
				return BadRequest("Invalid Request");
			IEnumerable<string> topics = queryObject.Query.Contains(',') ? queryObject.Query.Split(',') : new string[] {queryObject.Query};

			return Ok(await _queryLogic.SearchMediasByTopic(topics, _userContext.CurrentUser, instagramId, limit));
		}

		[HttpPost]
		[Route("api/query/searchMediaLocation/{instagramId}/{limit}")]
		public async Task<IActionResult> SearchMediasByLocation([FromBody] UserSearchRequest queryObject, string instagramId, int limit)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser) || string.IsNullOrEmpty(queryObject.Query))
				return BadRequest("Invalid Request");

			return Ok(await _queryLogic.SearchMediasByLocation(new QuarklessContexts.Models.Profiles.Location
			{
				City = queryObject.Query
			}, _userContext.CurrentUser, instagramId, limit));
		}

		[HttpPost]
		[Route("api/query/searchUserTopic/{instagramId}/{limit}")]
		public async Task<IActionResult> SearchUsersByTopic([FromBody] UserSearchRequest queryObject, string instagramId, int limit)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser) || string.IsNullOrEmpty(queryObject.Query) && queryObject.Query.Length<2)
				return BadRequest("Invalid Request");
			IEnumerable<string> topics = queryObject.Query.Contains(',') ? queryObject.Query.Split(',') : new string[] {queryObject.Query};

			return Ok(await _queryLogic.SearchUsersByTopic(topics, _userContext.CurrentUser, instagramId, limit));
		}

		[HttpPost]
		[Route("api/query/searchUserLocation/{instagramId}/{limit}")]
		public async Task<IActionResult> SearchUsersByLocation([FromBody] UserSearchRequest queryObject, string instagramId, int limit)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser) || string.IsNullOrEmpty(queryObject.Query))
				return BadRequest("Invalid Request");

			return Ok(await _queryLogic.SearchUsersByLocation(new QuarklessContexts.Models.Profiles.Location
			{
				City = queryObject.Query
			}, _userContext.CurrentUser, instagramId, limit));
		}


		#region Heartbeat Logic Stuff

		[HttpPost]
		[Route("api/query/recentComments")]
		public async Task<IActionResult> GetRecentComments(ProfileRequest profile)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			profile.AccountId = _userContext.CurrentUser;
			return Ok(await _queryLogic.GetRecentComments(profile));
		}

		[HttpPost]
		[Route("api/query/userInbox")]
		public async Task<IActionResult> GetUserInbox(ProfileRequest profile)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			profile.AccountId = _userContext.CurrentUser;
			return Ok(await _queryLogic.GetUserInbox(profile));
		}

		[HttpPost]
		[Route("api/query/userMedias")]
		public async Task<IActionResult> GetUsersMedias(ProfileRequest profile)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			profile.AccountId = _userContext.CurrentUser;
			return Ok(await _queryLogic.GetUsersMedia(profile));
		}

		[HttpPost]
		[Route("api/query/userFeed")]
		public async Task<IActionResult> GetUsersFeed(ProfileRequest profile)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			profile.AccountId = _userContext.CurrentUser;
			return Ok(await _queryLogic.GetUsersFeed(profile));
		}

		[HttpPost]
		[Route("api/query/userFollowerList")]
		public async Task<IActionResult> GetUserFollowerList(ProfileRequest profile)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			profile.AccountId = _userContext.CurrentUser;
			return Ok(await _queryLogic.GetUsersFollowerList(profile));
		}

		[HttpPost]
		[Route("api/query/userFollowingList")]
		public async Task<IActionResult> GetUserFollowingList(ProfileRequest profile)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			profile.AccountId = _userContext.CurrentUser;
			return Ok(await _queryLogic.GetUsersFollowingList(profile));
		}

		[HttpPost]
		[Route("api/query/userLocation")]
		public async Task<IActionResult> GetUserByLocation(ProfileRequest profile)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			profile.AccountId = _userContext.CurrentUser;
			return Ok(await _queryLogic.GetUserByLocation(profile));
		}	
		
		[HttpPost]
		[Route("api/query/userSuggestionFollowing")]
		public async Task<IActionResult> GetUsersSuggestedFollowingList(ProfileRequest profile)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			profile.AccountId = _userContext.CurrentUser;
			return Ok(await _queryLogic.GetUsersSuggestedFollowingList(profile));
		}

		[HttpPost]
		[Route("api/query/userTargetList")]
		public async Task<IActionResult> GetUsersTargetList(ProfileRequest profile)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			profile.AccountId = _userContext.CurrentUser;
			return Ok(await _queryLogic.GetUsersTargetList(profile));
		}

		[HttpPost]
		[Route("api/query/mediasLocation")]
		public async Task<IActionResult> GetMediasByLocation(ProfileRequest profile)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			profile.AccountId = _userContext.CurrentUser;
			return Ok(await _queryLogic.GetMediasByLocation(profile));
		}
		#endregion
	}
}
