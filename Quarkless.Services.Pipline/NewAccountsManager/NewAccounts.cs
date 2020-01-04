using Quarkless.InstagramCreator.Repository;
using QuarklessLogic.Logic.TopicLookupLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quarkless.Vision;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessRepositories.InstagramAccountRepository;
using QuarklessRepositories.ProfileRepository;

namespace Quarkless.Services.Pipeline.NewAccountsManager
{
	public interface ITest
	{
		Task TestStart();
	}
	public class Test : ITest
	{
		private readonly IVisionClient _client;
		public Test(IVisionClient client)
		{
			_client = client;
		}

		public async Task TestStart()
		{

		}
	}
	public class NewAccounts
	{
		private readonly IInstagramAccountCreatorRepository _accountCreatorRepository;
		private readonly ITopicLookupLogic _topicLookupLogic;
		private readonly IInstagramAccountRepository _instagramAccountRepository;
		private readonly IProfileRepository _profileRepository;
		public NewAccounts(IInstagramAccountCreatorRepository accountCreatorRepository, ITopicLookupLogic topicLookupLogic,
			IInstagramAccountRepository instagramAccountRepository, IProfileRepository profileRepository)
		{
			_accountCreatorRepository = accountCreatorRepository;
			_topicLookupLogic = topicLookupLogic;
			_instagramAccountRepository = instagramAccountRepository;
			_profileRepository = profileRepository;
		}

		public async Task FetchNewAccountsCreated(string accountId = "lemonkaces")
		{
			var allAccounts = await _accountCreatorRepository.GetAllInstagramAccounts();
			if (allAccounts == null || !allAccounts.Any()) return;


			foreach (var instagramAccount in allAccounts.Where(_=>_.Virgin))
			{
				var toInstagramModel = new InstagramAccountModel
				{
					AccountId = accountId,
					AgentState = (int) AgentState.WarmingUp,
					DateAdded = DateTime.UtcNow,
					Email = instagramAccount?.Email,
					EmailPassword = instagramAccount.Password,
					FullName = instagramAccount.FirstName + " " + instagramAccount.LastName,
					Password = instagramAccount.Password,
					Type = (int) 0,
					
				};
			}
		}
	}
}
