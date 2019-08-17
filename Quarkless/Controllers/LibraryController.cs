using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.Library;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.LibaryLogic;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class LibraryController : ControllerBase
	{
		private readonly ILibraryLogic _libraryLogic;
		private readonly IUserContext _userContext;

		public LibraryController(ILibraryLogic libraryLogic, IUserContext userContext)
		{
			_libraryLogic = libraryLogic;
			_userContext = userContext;
		}

		[HttpPost]
		[Route("api/library/savedCaptions/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> AddSavedCaptions(string accountId, string instagramAccountId,
			CaptionsLib captionsLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				await _libraryLogic.AddSavedCaptions(accountId, instagramAccountId, captionsLib);
				return Ok(new { IsSuccess = true });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		[HttpPost]
		[Route("api/library/savedHashtags/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> AddSavedHashtags(string accountId, string instagramAccountId,
			HashtagsLib hashtagsLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				await _libraryLogic.AddSavedHashtags(accountId, instagramAccountId, hashtagsLib);
				return Ok(new { IsSuccess = true });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		[HttpPost]
		[Route("api/library/savedMedias/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> AddSavedMedias(string accountId, string instagramAccountId,
			MediasLib mediasLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				await _libraryLogic.AddSavedMedias(accountId, instagramAccountId, mediasLib);
				return Ok(new { IsSuccess = true });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		
		[HttpPut]
		[Route("api/library/savedCaptions/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> DeleteSavedCaptions(string accountId, string instagramAccountId,
			CaptionsLib captionsLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				await _libraryLogic.DeleteSavedCaptions(accountId, instagramAccountId, captionsLib);
				return Ok(new { IsSuccess = true });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		[HttpPut]
		[Route("api/library/savedHashtags/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> DeleteSavedHashtags(string accountId, string instagramAccountId,
			HashtagsLib hashtagsLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				await _libraryLogic.DeleteSavedHashtags(accountId, instagramAccountId, hashtagsLib);
				return Ok(new { IsSuccess = true });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		[HttpPut]
		[Route("api/library/savedMedias/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> DeleteSavedMedias(string accountId, string instagramAccountId,
			MediasLib mediasLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				await _libraryLogic.DeleteSavedMedias(accountId, instagramAccountId, mediasLib);
				return Ok(new { IsSuccess = true });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpGet]
		[Route("api/library/savedMedias/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> GetSavedMedias(string accountId, string instagramAccountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.GetSavedMedias(accountId, instagramAccountId);
				return Ok(new { IsSuccess = true, Data = results });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpGet]
		[Route("api/library/savedCaptions/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> GetSavedCaptions(string accountId, string instagramAccountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.GetSavedCaptions(accountId, instagramAccountId);
				return Ok(new { IsSuccess = true, Data = results });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpGet]
		[Route("api/library/savedHashtags/{accountId}/{instagramAccountId}")]
		public async Task<IActionResult> GetSavedHashtags(string accountId, string instagramAccountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.GetSavedHashtags(accountId, instagramAccountId);
				return Ok(new { IsSuccess = true, Data = results });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
	}
}
