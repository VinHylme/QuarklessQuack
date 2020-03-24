using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Base.Analytics.Models.Interfaces;
using Quarkless.Base.Auth.Common.Models.Enums;
using Quarkless.Base.Auth.Common.Models.Interfaces;

namespace Quarkless.Controllers
{
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	[ApiController]
	public class AnalyticsController : ControllerBase
	{
		private readonly IHashtagsAnalytics _hashtagsAnalytics;
		private readonly IUserContext _userContext;
		public AnalyticsController(IHashtagsAnalytics hashtagsAnalytics, IUserContext userContext)
		{
			_hashtagsAnalytics = hashtagsAnalytics;
			_userContext = userContext;
		}

		[HttpGet]
		[Route("api/analytics/hashtag/{tagName}/{limit=0}")]
		public async Task<IActionResult> GetHashtagAnalysis(string tagName, int limit = 0)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request for user");

			var results = await _hashtagsAnalytics.GetHashtagAnalysis(tagName, limit);
			if (!results.IsSuccessful)
				return NotFound(results.Info);

			return Ok(results.Results);
		}
	}
}