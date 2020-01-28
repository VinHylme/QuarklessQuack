using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Quarkless.Models.Agent;
using Quarkless.Models.Agent.Interfaces;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Enums;
using Quarkless.Models.InstagramAccounts.Interfaces;

namespace Quarkless.Logic.Agent
{
	public class AgentLogic : IAgentLogic
	{
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		public AgentLogic(IInstagramAccountLogic instagramAccountLogic)
		{
			_instagramAccountLogic = instagramAccountLogic;
		}

		public async Task<IEnumerable<ShortInstagramAccountModel>> GetActiveAccounts()
			=> await _instagramAccountLogic.GetActiveAgentInstagramAccounts();
		
		public async Task<IEnumerable<ShortInstagramAccountModel>> GetAllAccounts(int type = 0)
			=> await _instagramAccountLogic.GetInstagramAccounts(type);
		
		public async Task<ShortInstagramAccountModel> GetAccount(string accountId, string instagramAccountId)
			=> await _instagramAccountLogic.GetInstagramAccountShort(accountId, instagramAccountId);

		public async Task<AgentResponse> Start(string accountId, string instagramAccountId)
		{
			var instagramAccountShort = await _instagramAccountLogic.GetInstagramAccountShort(accountId, instagramAccountId);

			await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId, instagramAccountId,
				new InstagramAccountModel
				{
					AgentState = (int)AgentState.Running
				});

			return new AgentResponse
			{
				HttpStatus = HttpStatusCode.OK,
				Message = $"Started for {accountId}, of {instagramAccountShort.Username}"
			};
		}
		public async Task<AgentResponse> Stop(string accountId, string instagramAccountId)
		{
			try
			{
				await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId, instagramAccountId,
					new InstagramAccountModel
					{
						AgentState = (int)AgentState.Stopped
					});
				return new AgentResponse
				{
					HttpStatus = HttpStatusCode.OK,
					Message = "Stopped"
				};
			}
			catch (Exception ee)
			{
				return new AgentResponse
				{
					HttpStatus = HttpStatusCode.InternalServerError,
					Message = ee.Message
				};
			}
		}
		public async Task<AgentResponse> ChangeAgentState(string accountId, string instagramAccountId, AgentState state)
		{
			try
			{
				await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId, instagramAccountId,
					new InstagramAccountModel
					{
						AgentState = (int) state
					});
				return new AgentResponse
				{
					HttpStatus = HttpStatusCode.OK,
					Message = $"Changed State to: {state} for {accountId}/{instagramAccountId}"
				};
			}
			catch (Exception ee)
			{
				return new AgentResponse
				{
					HttpStatus = HttpStatusCode.InternalServerError,
					Message = ee.Message
				};
			}
		}
		public async Task<AgentResponse> UpdateInstagramAccount(string accountId, string instagramAccountId,
			ShortInstagramAccountModel updatedAccount)
		{
			try
			{
				await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId, instagramAccountId,
					new InstagramAccountModel
					{
						AgentState = updatedAccount.AgentState,
						LastPurgeCycle = updatedAccount.LastPurgeCycle,
						ChallengeInfo = updatedAccount.ChallengeInfo,
						DateAdded = updatedAccount.DateAdded,
						SleepTimeRemaining = updatedAccount.SleepTimeRemaining,
						UserLimits = updatedAccount.UserLimits,

					});
				return new AgentResponse
				{
					HttpStatus = HttpStatusCode.OK,
					Message = $"Updated Account for {accountId}/{instagramAccountId}"
				};
			}
			catch (Exception ee)
			{
				return new AgentResponse
				{
					HttpStatus = HttpStatusCode.InternalServerError,
					Message = ee.Message
				};
			}
		}

	}
}
