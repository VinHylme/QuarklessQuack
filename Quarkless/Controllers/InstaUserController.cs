﻿using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Auth.AuthTypes;
using QuarklessContexts.Contexts;
using QuarklessLogic.Logic.InstaUserLogic;
using System.Threading.Tasks;

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
		public InstaUserController(IUserContext userContext, IInstaUserLogic instaUserLogic)
		{
			_userContext = userContext;
			_instaUserLogic = instaUserLogic;
		}

		[HttpGet]
		[Route("api/instaUser/recentActivityFeed/{limit}")]
		public async Task<IActionResult> GetRecentActivityFeedAsync(int limit=2)
		{
			if (_userContext.UserAccountExists) { 
				var res = await _instaUserLogic.GetRecentActivityFeedAsync(limit);
				if (res.Succeeded) { 
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/acceptFriendShip/{userId}")]
		public async Task<IActionResult> AcceptFriendshipRequestAsync(long userId)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.AcceptFriendshipRequestAsync(userId);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/blockUser/{userId}")]
		public async Task<IActionResult> BlockUser(long userId)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.BlockUser(userId);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/favoriteUser/{userId}")]
		public async Task<IActionResult> FavoriteUser(long userId)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.FavoriteUser(userId);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/favoriteUserStories/{userId}")]
		public async Task<IActionResult> FavoriteUserStories(long userId)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.FavoriteUserStories(userId);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpPost]
		[Route("api/instaUser/followUser/{userId}")]
		public async Task<IActionResult> FollowUser(long userId)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.FollowUser(userId);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/currentUserFollowers/{limit}")]
		public async Task<IActionResult> GetCurrentUserFollowers(int limit=2)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetCurrentUserFollowers(limit);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/followingActicityFeed/{limit}")]
		public async Task<IActionResult> GetFollowingActivityFeed(int limit)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetFollowingActivityFeed(limit);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpPut]
		[Route("api/instaUser/friendshipStatuses")]
		public async Task<IActionResult> GetFriendshipStatuses([FromBody] long[] userIds)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetFriendshipStatuses(userIds);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/fullUserInfo/{userId}")]
		public async Task<IActionResult> GetFullUserInfo(long userId)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetFullUserInfo(userId);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/pendingFriend")]
		public async Task<IActionResult> GetPendingFriendRequest()
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetPendingFriendRequest();
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpPut]
		[Route("api/instaUser/suggestionDetails")]
		public async Task<IActionResult> GetSuggestionDetails([FromBody] long[] userIds)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetSuggestionDetails(userIds);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/user/{username}")]
		public async Task<IActionResult> GetUser(string username)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetUser(username);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/userFollowers/{username}/{limit}/{query}/{mutualfirst}")]
		public async Task<IActionResult> GetUserFollowers(string username, int limit=2, string query = "", bool mutalfirst = false)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetUserFollowers(username,limit, query, mutalfirst);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/userFollowing/{username}/{limit}/{query}")]
		public async Task<IActionResult> GetUserFollowing(string username, int limit=2, string query = "")
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetUserFollowing(username, limit, query);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/userInfo/{userPk}")]
		public async Task<IActionResult> GetUserInfo(long userPk)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetUserInfo(userPk);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/userInfoUsername/{username}")]
		public async Task<IActionResult> GetUserInfoUsername(string username)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetUserInfoUsername(username);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/userMedia/{username}/{limit}")]
		public async Task<IActionResult> GetUserMedia(string username, int limit=2)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetUserMedia(username,limit);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpPut]
		[Route("api/instaUser/userNametag")]
		public async Task<IActionResult> GetUserNametag([FromBody] InstaImage nametagmage)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetUserNametag(nametagmage);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/userTags/{username}/{limit}")]
		public async Task<IActionResult> GetUserTags(string username, int limit=2)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetUserTags(username, limit);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/userShoppableMedia/{username}/{limit}")]
		public async Task<IActionResult> GetUserShoppableMedia(string username, int limit=2)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetUserShoppableMedia(username,limit);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/userSuggestions/{limit}")]
		public async Task<IActionResult> GetUserSuggestions(int limit=2)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.GetUserSuggestions(limit);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/hideStoryFromUser/{userId}")]
		public async Task<IActionResult> HideStoryFromUser(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.HideStoryFromUser(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/ignoreFriendship/{userId}")]
		public async Task<IActionResult> IgnoreFriendship(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.IgnoreFriendship(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/markUserAsOverage/{userId}")]
		public async Task<IActionResult> MarkUserAsOverage(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.MarkUserAsOverage(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/muteFriendStory/{userId}")]
		public async Task<IActionResult> MuteFriendStory(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.MuteFriendStory(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/MuteUserMedia/{userId}/{muteOption}")]
		public async Task<IActionResult> MuteUserMedia(long userid, int muteOption = 1)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.MuteUserMedia(userid, (InstaMuteOption) muteOption);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/removeFollower/{userId}")]
		public async Task<IActionResult> RemoveFollower(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.RemoveFollower(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/reportUser/{userId}")]
		public async Task<IActionResult> ReportUser(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.ReportUser(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/translateBio/{userId}")]
		public async Task<IActionResult> TranslateBio(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.TranslateBio(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/unblockUser/{userId}")]
		public async Task<IActionResult> UnBlockUser(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.UnBlockUser(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/unfavoriteUser/{userId}")]
		public async Task<IActionResult> UnFavoriteUser(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.UnFavoriteUser(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/unfavoriteUserStories/{userId}")]
		public async Task<IActionResult> UnFavoriteUserStories(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.UnFavoriteUserStories(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/unFollowUser/{userId}")]
		public async Task<IActionResult> UnFollowUser(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.UnFollowUser(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/unhideMyStory/{userId}")]
		public async Task<IActionResult> UnHideMyStoryFromUser(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.UnHideMyStoryFromUser(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/unmuteFriendStory/{userId}")]
		public async Task<IActionResult> UnMuteFriendStory(long userid)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.UnMuteFriendStory(userid);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}

		[HttpGet]
		[Route("api/instaUser/unmuteUserMedia/{userId}/{muteOption}")]
		public async Task<IActionResult> UnMuteUserMedia(long userid, int muteOption)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _instaUserLogic.UnMuteUserMedia(userid, (InstaMuteOption) muteOption);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				else
				{
					return NotFound(res.Info);
				}
			}
			else
			{
				return BadRequest("Invalid Id");
			}
		}
	}
}
