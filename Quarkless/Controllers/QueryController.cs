using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.Logic.QueryLogic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
			return Ok(await _queryLogic.GetReleatedKeywords(topic));
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
		[Route("api/query/search/similar/{limit}/{offset=0}")]
		public async Task<IActionResult> SimilarSearch([FromBody] IEnumerable<string> urls, [FromRoute] int limit, [FromRoute] int offset = 0)
		{
			if(string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			return Ok(await _queryLogic.SimilarImagesSearch(_userContext.CurrentUser, limit, offset, urls));
		}
	}
}
