using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.QueryLogic;
using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.QueryModels;

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
		[Route("api/query/releated/{topic}")]
		public async Task<IActionResult> SearchReleatedTopic(string topic)
		{
			if (!_userContext.UserAccountExists && string.IsNullOrEmpty(topic))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetRelatedKeywords(topic));
		}

		[HttpGet]
		[Route("api/query/build/tags/{topic}/{subcategory}/{language}/{limit}/{pickRate}")]
		public async Task<IActionResult> BuildTags(string topic, string subcategory, 
			string language, int limit, int pickRate)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.BuildHashtags(topic, subcategory, language, limit, pickRate));
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

		[HttpGet]
		[Route("api/query/recentComments/{instagramId}/{topic}")]
		public async Task<IActionResult> GetRecentComments(string instagramId, string topic)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetRecentComments(_userContext.CurrentUser, instagramId, topic));
		}

		[HttpGet]
		[Route("api/query/userInbox/{instagramId}/{topic}")]
		public async Task<IActionResult> GetUserInbox(string instagramId, string topic)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetUserInbox(_userContext.CurrentUser, instagramId, topic));
		}

		[HttpGet]
		[Route("api/query/userMedias/{instagramId}/{topic}")]
		public async Task<IActionResult> GetUsersMedias(string instagramId, string topic)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetUsersMedia(_userContext.CurrentUser, instagramId, topic));
		}

		[HttpGet]
		[Route("api/query/userFeed/{instagramId}/{topic}")]
		public async Task<IActionResult> GetUsersFeed(string instagramId, string topic)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetUsersFeed(_userContext.CurrentUser, instagramId, topic));
		}

		[HttpGet]
		[Route("api/query/userFollowerList/{instagramId}/{topic}")]
		public async Task<IActionResult> GetUserFollowerList(string instagramId, string topic)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetUsersFollowerList(_userContext.CurrentUser, instagramId, topic));
		}

		[HttpGet]
		[Route("api/query/userFollowingList/{instagramId}/{topic}")]
		public async Task<IActionResult> GetUserFollowingList(string instagramId, string topic)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetUsersFollowingList(_userContext.CurrentUser, instagramId, topic));
		}

		[HttpGet]
		[Route("api/query/userLocation/{instagramId}/{topic}")]
		public async Task<IActionResult> GetUserByLocation(string instagramId, string topic)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetUserByLocation(_userContext.CurrentUser, instagramId, topic));
		}	
		
		[HttpGet]
		[Route("api/query/userSuggestionFollowing/{instagramId}/{topic}")]
		public async Task<IActionResult> GetUsersSuggestedFollowingList(string instagramId, string topic)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetUsersSuggestedFollowingList(_userContext.CurrentUser, instagramId, topic));
		}

		[HttpGet]
		[Route("api/query/userTargetList/{instagramId}/{topic}")]
		public async Task<IActionResult> GetUsersTargetList(string instagramId, string topic)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetUsersTargetList(_userContext.CurrentUser, instagramId, topic));
		}

		[HttpGet]
		[Route("api/query/mediasLocation/{instagramId}/{topic}")]
		public async Task<IActionResult> GetMediasByLocation(string instagramId, string topic)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.GetMediasByLocation(_userContext.CurrentUser, instagramId, topic));
		}
		#endregion
	}
}
