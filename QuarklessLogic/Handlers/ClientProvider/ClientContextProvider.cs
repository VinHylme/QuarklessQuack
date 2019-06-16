﻿using InstagramApiSharp.API.Processors;
using InstagramApiSharp.Classes;
using Newtonsoft.Json;
using QuarklessContexts.InstaClient;
using QuarklessContexts.Models;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.Logic.ProxyLogic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QuarklessLogic.Handlers.ClientProvider
{
	public class ClientContextProvider : IClientContextProvider
	{
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IProxyLogic _proxyLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly IReportHandler _reportHandler;
		private readonly IInstaClient _instaClient;
		public ClientContextProvider(IInstagramAccountLogic instagramAccountLogic,
			IProfileLogic profileLogic, IProxyLogic proxyLogic, IReportHandler reportHandler, IInstaClient instaClient)
		{
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_proxyLogic = proxyLogic;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("/Logic/ClientContextProvider");
			_instaClient = instaClient;
		}

		public async Task<ContextContainer> Get(string accId, string insAccId)
		{
			return await GetClient(accId, insAccId);
		}

		public InstaClient InitialClientGenerate()
		{
			try
			{
				return this._instaClient.Empty();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		private async Task<ContextContainer> GetClient(string accountId, string instagramAccountId)
		{
			try
			{
				var instaAcc = await _instagramAccountLogic.GetInstagramAccount(accountId, instagramAccountId);
				if (instaAcc == null)
				{
					return null;
				}
				var profileOfInstaAcc = await _profileLogic.GetProfile(accountId, instagramAccountId);
				var proxyOfInstaAcc = await _proxyLogic.GetProxyAssignedTo(accountId, instagramAccountId);

				var client = _instaClient.GetClientFromModel(new InstagramClientAccount
				{
					InstagramAccount = instaAcc,
					Profile = profileOfInstaAcc,
					//Proxy = proxyOfInstaAcc
				});

				if (client != null && client.ReturnClient != null)
				{
					var stateExtracted = JsonConvert.DeserializeObject<StateData>(await client.GetStateDataFromString());

					if (stateExtracted != instaAcc.State)
					{
						await _instagramAccountLogic.PartialUpdateInstagramAccount(instagramAccountId, new InstagramAccountModel
						{
							State = stateExtracted
						});
					}
					return new ContextContainer
					{
						ActionClient = client.ReturnClient,
						InstagramAccount = instaAcc,
						Profile = profileOfInstaAcc
					};
				}
				else
				{
					_reportHandler.MakeReport($"GetClientFor user: {accountId}, insta: {instagramAccountId} failed, client returned nothing");
					return null;
				}
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
	}
}
