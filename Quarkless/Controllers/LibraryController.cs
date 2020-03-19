using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Base.Auth.Common.Models.Enums;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.Library.Models;
using Quarkless.Base.Library.Models.Interfaces;

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

		#region Add
		[HttpPost]
		[Route("api/library/savedMessages")]
		public async Task<IActionResult> AddSavedMessages(MessagesLib messagesLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.AddSavedMessages(messagesLib);
				return Ok(results ? new { IsSuccess = true } : new { IsSuccess = false });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpPost]
		[Route("api/library/savedCaptions")]
		public async Task<IActionResult> AddSavedCaptions(CaptionsLib captionsLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.AddSavedCaptions(captionsLib);
				return Ok(results ? new { IsSuccess = true } : new { IsSuccess = false });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		
		[HttpPost]
		[Route("api/library/savedHashtags")]
		public async Task<IActionResult> AddSavedHashtags(HashtagsLib hashtagsLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.AddSavedHashtags(hashtagsLib);
				return Ok(results ? new { IsSuccess = true } : new { IsSuccess = false });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		
		[HttpPost]
		[Route("api/library/savedMedias")]
		public async Task<IActionResult> AddSavedMedias(IEnumerable<MediasLib> mediasLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.AddSavedMedias(mediasLib);
				return Ok(results ? new { IsSuccess = true } : new { IsSuccess = false });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		#endregion
		#region Update
		[HttpPut]
		[Route("api/library/savedMessages")]
		public async Task<IActionResult> UpdateSavedMessages(MessagesLib messagesLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.UpdateSavedMessage(messagesLib);
				return Ok(results ? new { IsSuccess = true } : new { IsSuccess = false });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpPut]
		[Route("api/library/savedCaptions")]
		public async Task<IActionResult> UpdateSavedCaptions(CaptionsLib captionsLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.UpdateSavedCaptions(captionsLib);
				return Ok(results ? new { IsSuccess = true } : new { IsSuccess = false });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		
		[HttpPut]
		[Route("api/library/savedHashtags")]
		public async Task<IActionResult> UpdateSavedHashtags(HashtagsLib hashtagsLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.UpdateSavedHashtags(hashtagsLib);
				return Ok(results ? new { IsSuccess = true } : new { IsSuccess = false });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		#endregion
		#region Delete
		[HttpPut]
		[Route("api/library/delete/savedMessages")]
		public async Task<IActionResult> DeleteSavedMessages(MessagesLib messagesLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.DeleteSavedMessage(messagesLib);
				return Ok(results ? new { IsSuccess = true } : new { IsSuccess = false });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpPut]
		[Route("api/library/delete/savedCaptions")]
		public async Task<IActionResult> DeleteSavedCaptions(CaptionsLib captionsLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.DeleteSavedCaptions(captionsLib);
				return Ok(results ? new { IsSuccess = true } : new { IsSuccess = false });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpPut]
		[Route("api/library/delete/savedHashtags")]
		public async Task<IActionResult> DeleteSavedHashtags(HashtagsLib hashtagsLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.DeleteSavedHashtags(hashtagsLib);
				return Ok(results ? new { IsSuccess = true } : new {IsSuccess = false});
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpPut]
		[Route("api/library/delete/savedMedias")]
		public async Task<IActionResult> DeleteSavedMedias(MediasLib mediasLib)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.DeleteSavedMedias(mediasLib);
				return Ok(results ? new { IsSuccess = true } : new {IsSuccess = false});
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		#endregion
		#region Get
		[HttpGet]
		[Route("api/library/savedMessagesForUser/{instagramAccountId}")]
		public async Task<IActionResult> GetSavedMessagesForUser(string instagramAccountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.GetSavedMessagesForUser(instagramAccountId);
				return Ok(new { IsSuccess = true, Data = results });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		[HttpGet]
		[Route("api/library/savedMediasForUser/{instagramAccountId}")]
		public async Task<IActionResult> GetSavedMediasForUser(string instagramAccountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.GetSavedMediasForUser(instagramAccountId);
				return Ok(new { IsSuccess = true, Data = results });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpGet]
		[Route("api/library/savedCaptionsForUser/{instagramAccountId}")]
		public async Task<IActionResult> GetSavedCaptionsForUser(string instagramAccountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.GetSavedCaptionsForUser(instagramAccountId);
				return Ok(new { IsSuccess = true, Data = results });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpGet]
		[Route("api/library/savedHashtagsForUser/{instagramAccountId}")]
		public async Task<IActionResult> GetSavedHashtagsForUser(string instagramAccountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.GetSavedHashtagsForUser(instagramAccountId);
				return Ok(new { IsSuccess = true, Data = results });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpGet]
		[Route("api/library/savedMessages/{accountId}")]
		public async Task<IActionResult> GetSavedMessages(string accountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.GetSavedMessages(accountId);
				return Ok(new { IsSuccess = true, Data = results });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpGet]
		[Route("api/library/savedMedias/{accountId}")]
		public async Task<IActionResult> GetSavedMedias(string accountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.GetSavedMedias(accountId);
				return Ok(new { IsSuccess = true, Data = results });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpGet]
		[Route("api/library/savedCaptions/{accountId}")]
		public async Task<IActionResult> GetSavedCaptions(string accountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.GetSavedCaptions(accountId);
				return Ok(new { IsSuccess = true, Data = results });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}

		[HttpGet]
		[Route("api/library/savedHashtags/{accountId}")]
		public async Task<IActionResult> GetSavedHashtags(string accountId)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			try
			{
				var results = await _libraryLogic.GetSavedHashtags(accountId);
				return Ok(new { IsSuccess = true, Data = results });
			}
			catch (Exception ee)
			{
				return BadRequest(new {IsSuccess = false, Expection = ee.Message});
			}
		}
		#endregion
	}
}
