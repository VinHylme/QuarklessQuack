using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quarkless.Analyser;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Agent.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Library.Interfaces;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Services.Automation.Models.Tests;
using Quarkless.Models.Storage.Interfaces;

namespace Quarkless.Logic.Services.Automation
{
	public class AgentTests : IAgentTests
	{
		private readonly IActionCommitFactory _actionCommitFactory;
		private readonly IActionExecuteFactory _actionExecuteFactory;
		private readonly IAgentLogic _agentLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly ILibraryLogic _libraryLogic;
		private readonly IPostAnalyser _postAnalyser;
		private readonly IS3BucketLogic _bucketLogic;

		public AgentTests(IActionCommitFactory actionCommitFactory, IAgentLogic agentLogic,
			IProfileLogic profileLogic, ILibraryLogic libraryLogic, IPostAnalyser postAnalyser,
			IS3BucketLogic s3BucketLogic, IActionExecuteFactory actionExecuteFactory)
		{
			_actionCommitFactory = actionCommitFactory;
			_actionExecuteFactory = actionExecuteFactory;
			_agentLogic = agentLogic;
			_profileLogic = profileLogic;
			_libraryLogic = libraryLogic;
			_postAnalyser = postAnalyser;
			_bucketLogic = s3BucketLogic;
		}

		private async Task<UserStoreDetails> GetUserDetail()
		{
			const string accountId = "lemonkaces";
			const string instagramAccountId = "5d364dbaa2b9a40f649069a6";
			var testAccount = await _agentLogic.GetAccount(accountId, instagramAccountId);
			var profile = await _profileLogic.GetProfile(accountId, instagramAccountId);

			var user = new UserStoreDetails
			{
				AccountId = accountId,
				InstagramAccountUsername = testAccount.Username,
				InstagramAccountUser = testAccount.Id,
				Profile = profile,
				ShortInstagram = testAccount,
				MessagesTemplates = await _libraryLogic.GetSavedMessages(accountId)
			};
			return user;
		}
		private async Task<TestResponse> TestReactStoryAction(bool execute = true)
		{
			var response = new TestResponse();
			var user = await GetUserDetail();

			var postAction = _actionCommitFactory.Create(ActionType.ReactStory, user)
				.ModifyOptions(new ReactStoryOptions(StoryActionType.WatchFromFeed));

			var results = await postAction.PushAsync(DateTimeOffset.UtcNow);

			if (!results.IsSuccessful)
			{
				response.PassedInActionBuild = false;
				response.Error = results.Info;
				return response;
			}

			if (!execute)
			{
				response.IsSuccessful = true;
				response.PassedInActionBuild = true;
				response.Items = results.Results.DataObjects;
				return response;
			}

			var resultsExecute = await _actionExecuteFactory.Create(ActionType.ReactStory,
				new UserStore
				{
					AccountId = user.AccountId,
					InstagramAccountUser = user.InstagramAccountUser,
					InstagramAccountUsername = user.InstagramAccountUsername
				}).ExecuteAsync(new EventExecuteBody(results.Results.DataObjects.First().Body,
				results.Results.DataObjects.First().BodyType));

			if (!resultsExecute.IsSuccessful)
			{
				response.PassedInActionBuild = true;
				response.PassedInActionExecute = false;
				response.Error = resultsExecute.Info;
				return response;
			}

			response.PassedInActionBuild = true;
			response.PassedInActionExecute = true;
			response.IsSuccessful = true;
			response.Items = results.Results.DataObjects;
			return response;
		}
		private async Task<TestResponse> TestCreateMediaAction(bool execute = true)
		{
			var response = new TestResponse();
			var user = await GetUserDetail();

			var actionType = ActionType.CreatePost;

			var postAction = _actionCommitFactory.Create(actionType, user)
				.ModifyOptions(new PostActionOptions(_postAnalyser, _bucketLogic, 
					postMediaAction: PostMediaActionType.Carousel));

			var results = await postAction.PushAsync(DateTimeOffset.UtcNow);

			if (!results.IsSuccessful)
			{
				response.PassedInActionBuild = false;
				response.Error = results.Info;
				return response;
			}

			if (!execute)
			{
				response.IsSuccessful = true;
				response.PassedInActionBuild = true;
				response.Items = results.Results.DataObjects;
				return response;
			}

			var resultsExecute = await _actionExecuteFactory.Create(actionType,
				new UserStore
				{
					AccountId = user.AccountId,
					InstagramAccountUser = user.InstagramAccountUser,
					InstagramAccountUsername = user.InstagramAccountUsername
				}).ExecuteAsync(new EventExecuteBody(results.Results.DataObjects.First().Body,
				results.Results.DataObjects.First().BodyType));

			if (!resultsExecute.IsSuccessful)
			{
				response.PassedInActionBuild = true;
				response.PassedInActionExecute = false;
				response.Error = resultsExecute.Info;
				return response;
			}

			response.PassedInActionBuild = true;
			response.PassedInActionExecute = true;
			response.IsSuccessful = true;
			response.Items = results.Results.DataObjects;
			return response;
		}

		public async Task StartTests()
		{
			// var storyReactionTest = await TestReactStoryAction(false);
			// if (!storyReactionTest.IsSuccessful)
			// {
			//
			// }

			var postActionTest = await TestCreateMediaAction(false);
			if (!postActionTest.IsSuccessful)
			{

			}
		}
	}
}
