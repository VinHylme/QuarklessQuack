using System;
using System.IO;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Quarkless.Base.AccountOptions;
using Quarkless.Base.InstagramUser;
using Quarkless.Base.InstagramUser.Models;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Proxy;
using Quarkless.Models.ResponseResolver.Interfaces;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class InstaUserController : ControllerBase
	{
		private readonly IUserContext _userContext;
		private readonly IInstaUserLogic _instaUserLogic;
		private readonly IResponseResolver _responseResolver;
		private readonly IInstaAccountOptionsLogic _instaAccountOptions;
		public InstaUserController(IUserContext userContext, 
			IInstaUserLogic instaUserLogic,
			IInstaAccountOptionsLogic instaAccountOptions,
			IResponseResolver responseResolver)
		{
			_userContext = userContext;
			_instaUserLogic = instaUserLogic;
			_instaAccountOptions = instaAccountOptions;
			_responseResolver = responseResolver;
		}

		[HttpPut]
		[Route("api/account/changeBio")]
		public async Task<IActionResult> ChangeBiography(ChangeBiographyRequest biographyRequest)
		{
			if (!_userContext.UserAccountExists && string.IsNullOrEmpty(biographyRequest.Biography)) return BadRequest("Invalid Request");
			try
			{
				var results = await _responseResolver.WithResolverAsync(
					await _instaAccountOptions.SetBiographyAsync(biographyRequest.Biography), ActionType.CreateBiography, biographyRequest.ToJsonString());
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return BadRequest(results.Info);
			}
			catch (Exception ee)
			{
				return BadRequest("Failed" + ee.Message);
			}
		}
		[HttpPut]
		[Route("api/account/changepp")]
		public async Task<IActionResult> ChangeProfilePicture()
		{
			if (!_userContext.UserAccountExists)
				return BadRequest("Invalid Request");
			try
			{
				if (Request.Form.Files == null || Request.Form.Files.Count <= 0) return BadRequest("No files");
				var file = Request.Form.Files.FirstOrDefault();
				if (file == null) return BadRequest("No File attached");
				using (var ms = new MemoryStream())
				{
					file.CopyTo(ms);
					var bytes = ms.ToArray();

					var results = await _responseResolver.WithResolverAsync(
						await _instaAccountOptions.ChangeProfilePictureAsync(bytes),
						ActionType.ChangeProfilePicture, file.FileName);
					if (results.Succeeded)
					{
						return Ok(results.Value);
					}

					return BadRequest(results.Info);
				}
			}
			catch (Exception ee)
			{
				return BadRequest(ee.Message);
			}
		}

		[HttpPost]
		[Route("api/instaUser/createAccount/")]
		public async Task<IActionResult> CreateInstaAccount([FromBody] ProxyModel proxy)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser))
				return BadRequest("Invalid Request");
			var results = await _instaUserLogic.CreateAccount(proxy);
			if (results == null) return BadRequest("Failed");
			if (results.InResult.Succeeded)
			{
				return Ok(results);
			}
			return BadRequest(results.InResult.Info);
		}

		[HttpGet]
		[Route("api/instaUser/recentActivityFeed/{limit}")]
		public async Task<IActionResult> GetRecentActivityFeedAsync(int limit=2)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetRecentActivityFeedAsync(limit), ActionType.RecentActivityFeed, "");
			if (res.Succeeded) { 
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/acceptFriendShip/{userId}")]
		public async Task<IActionResult> AcceptFriendshipRequestAsync(long userId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.AcceptFriendshipRequestAsync(userId), ActionType.None, userId.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}

			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/blockUser/{userId}")]
		public async Task<IActionResult> BlockUser(long userId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.BlockUser(userId), ActionType.None, userId.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}

			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/favoriteUser/{userId}")]
		public async Task<IActionResult> FavoriteUser(long userId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.FavoriteUser(userId), ActionType.None, userId.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/favoriteUserStories/{userId}")]
		public async Task<IActionResult> FavoriteUserStories(long userId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.FavoriteUserStories(userId), ActionType.None, userId.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}

			return NotFound(res.Info);
		}

		[HttpPost]
		[Route("api/instaUser/followUser/{userId}")]
		public async Task<IActionResult> FollowUser(long userId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.FollowUser(userId), ActionType.FollowUser, userId.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/currentUserFollowers/{limit}")]
		public async Task<IActionResult> GetCurrentUserFollowers(int limit=2)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetCurrentUserFollowers(limit), ActionType.None, "");
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/followingActicityFeed/{limit}")]
		public async Task<IActionResult> GetFollowingActivityFeed(int limit)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetFollowingActivityFeed(limit), ActionType.None, "");
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res?.Info);
		}

		[HttpPut]
		[Route("api/instaUser/friendshipStatuses")]
		public async Task<IActionResult> GetFriendshipStatuses([FromBody] long[] userIds)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res =await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetFriendshipStatuses(userIds), ActionType.None, userIds.ToJsonString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/fullUserInfo/{userId}")]
		public async Task<IActionResult> GetFullUserInfo(long userId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetFullUserInfo(userId), ActionType.None, userId.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}

			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/pendingFriend")]
		public async Task<IActionResult> GetPendingFriendRequest()
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetPendingFriendRequest(), ActionType.None, "");
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}

			return NotFound(res.Info);
		}

		[HttpPut]
		[Route("api/instaUser/suggestionDetails/{userId}")]
		public async Task<IActionResult> GetSuggestionDetails(long userId, [FromBody] long[] userIds)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetSuggestionDetails(userId,userIds), ActionType.None, userId.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/user/{username}")]
		public async Task<IActionResult> GetUser(string username)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetUser(username), ActionType.None, username);
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/userFollowers/{username}/{limit}/{query}/{mutualfirst}")]
		public async Task<IActionResult> GetUserFollowers(string username, int limit=2, string query = "", bool mutalfirst = false)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetUserFollowers(username,limit, query, mutalfirst), ActionType.None, username);
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/userFollowing/{username}/{limit}/{query}")]
		public async Task<IActionResult> GetUserFollowing(string username, int limit=2, string query = "")
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetUserFollowing(username, limit, query), ActionType.None, username);
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/userInfo/{userPk}")]
		public async Task<IActionResult> GetUserInfo(long userPk)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetUserInfo(userPk), ActionType.None, userPk.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/userInfoUsername/{username}")]
		public async Task<IActionResult> GetUserInfoUsername(string username)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetUserInfoUsername(username), ActionType.None, username);
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/userMedia/{username}/{limit}")]
		public async Task<IActionResult> GetUserMedia(string username, int limit=2)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetUserMedia(username,limit), ActionType.None, username);
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpPut]
		[Route("api/instaUser/userNametag")]
		public async Task<IActionResult> GetUserNametag([FromBody] InstaImage nametagmage)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetUserNametag(nametagmage), ActionType.None, nametagmage.ToJsonString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/userTags/{username}/{limit}")]
		public async Task<IActionResult> GetUserTags(string username, int limit=2)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetUserTags(username, limit), ActionType.None, username);
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/userShoppableMedia/{username}/{limit}")]
		public async Task<IActionResult> GetUserShoppableMedia(string username, int limit=2)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetUserShoppableMedia(username,limit), ActionType.None, username);
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/userSuggestions/{limit}")]
		public async Task<IActionResult> GetUserSuggestions(int limit=2)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.GetUserSuggestions(limit), ActionType.None, "");
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/hideStoryFromUser/{userId}")]
		public async Task<IActionResult> HideStoryFromUser(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.HideStoryFromUser(userid), ActionType.None, "");
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/ignoreFriendship/{userId}")]
		public async Task<IActionResult> IgnoreFriendship(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.IgnoreFriendship(userid), ActionType.None, "");
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/markUserAsOverage/{userId}")]
		public async Task<IActionResult> MarkUserAsOverage(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.MarkUserAsOverage(userid), ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/muteFriendStory/{userId}")]
		public async Task<IActionResult> MuteFriendStory(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.MuteFriendStory(userid), ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/MuteUserMedia/{userId}/{muteOption}")]
		public async Task<IActionResult> MuteUserMedia(long userid, int muteOption = 1)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.MuteUserMedia(userid, (InstaMuteOption) muteOption), ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/removeFollower/{userId}")]
		public async Task<IActionResult> RemoveFollower(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.RemoveFollower(userid), ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/reportUser/{userId}")]
		public async Task<IActionResult> ReportUser(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.ReportUser(userid), ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/translateBio/{userId}")]
		public async Task<IActionResult> TranslateBio(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.TranslateBio(userid), ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/unblockUser/{userId}")]
		public async Task<IActionResult> UnBlockUser(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.UnBlockUser(userid), ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/unfavoriteUser/{userId}")]
		public async Task<IActionResult> UnFavoriteUser(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.UnFavoriteUser(userid), ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/unfavoriteUserStories/{userId}")]
		public async Task<IActionResult> UnFavoriteUserStories(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver
				.WithResolverAsync(await _instaUserLogic.UnFavoriteUserStories(userid), ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpPost]
		[Route("api/instaUser/unFollowUser/{userId}")]
		public async Task<IActionResult> UnFollowUser(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver.WithResolverAsync(await _instaUserLogic.UnFollowUser(userid), 
				ActionType.UnFollowUser, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/unhideMyStory/{userId}")]
		public async Task<IActionResult> UnHideMyStoryFromUser(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver.WithResolverAsync(await _instaUserLogic.UnHideMyStoryFromUser(userid),
				ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/unmuteFriendStory/{userId}")]
		public async Task<IActionResult> UnMuteFriendStory(long userid)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver.WithResolverAsync(await _instaUserLogic.UnMuteFriendStory(userid), 
				ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}
			return NotFound(res.Info);
		}

		[HttpGet]
		[Route("api/instaUser/unmuteUserMedia/{userId}/{muteOption}")]
		public async Task<IActionResult> UnMuteUserMedia(long userid, int muteOption)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Id");
			var res = await _responseResolver.WithResolverAsync(
				await _instaUserLogic.UnMuteUserMedia(userid, (InstaMuteOption) muteOption), ActionType.None, userid.ToString());
			if (res.Succeeded)
			{
				return Ok(res.Value);
			}

			return NotFound(res.Info);

		}
	}
}
