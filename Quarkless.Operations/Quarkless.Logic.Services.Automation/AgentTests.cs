using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Quarkless.Analyser;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Agent.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Library.Interfaces;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Storage.Interfaces;

namespace Quarkless.Logic.Services.Automation
{
	public class AgentTests : IAgentTests
	{
		private readonly IActionCommitFactory _actionCommitFactory;
		private readonly IAgentLogic _agentLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly ILibraryLogic _libraryLogic;
		private readonly IPostAnalyser _postAnalyser;
		private readonly IS3BucketLogic _bucketLogic;
		public AgentTests(IActionCommitFactory actionCommitFactory, IAgentLogic agentLogic,
			IProfileLogic profileLogic, ILibraryLogic libraryLogic, IPostAnalyser postAnalyser,
			IS3BucketLogic s3BucketLogic)
		{
			_actionCommitFactory = actionCommitFactory;
			_agentLogic = agentLogic;
			_profileLogic = profileLogic;
			_libraryLogic = libraryLogic;
			_postAnalyser = postAnalyser;
			_bucketLogic = s3BucketLogic;
		}

		public async Task StartTests()
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

			var postAction = _actionCommitFactory.Create(ActionType.CreatePost, user)
				.ModifyOptions(new PostActionOptions(_postAnalyser, _bucketLogic,
					postMediaAction: PostMediaActionType.Carousel));

			var results = await postAction.PushAsync(DateTimeOffset.UtcNow);
		}
	}
}
