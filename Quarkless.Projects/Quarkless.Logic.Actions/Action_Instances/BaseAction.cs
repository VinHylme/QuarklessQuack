﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Lookup;
using Quarkless.Models.Lookup.Enums;
using Quarkless.Models.Lookup.Interfaces;

namespace Quarkless.Logic.Actions.Action_Instances
{
	public class BaseAction
	{
		private readonly ILookupLogic _lookupLogic;
		private readonly ActionType _actionType;
		private readonly UserStoreDetails _user;
		public BaseAction(ILookupLogic lookupLogic, ActionType actionType, UserStoreDetails user)
		{
			_lookupLogic = lookupLogic;
			_actionType = actionType;
			_user = user;
		}

		public async Task AddObjectToLookup(string objectId)
		{
			await _lookupLogic.AddObjectToLookup(_user.AccountId, _user.InstagramAccountUser,
				new LookupModel(objectId)
			{
				Id = Guid.NewGuid().ToString(),
				LastModified = DateTime.UtcNow,
				LookupStatus = LookupStatus.Completed,
				ActionType = _actionType
			});
		}

		public async Task<List<LookupModel>> GetLookupItems()
		{
			var result = (await _lookupLogic.Get(_user.AccountId, _user.InstagramAccountUser))
				?.Where(_ =>  _.LookupStatus == LookupStatus.Completed && _.ActionType == _actionType);

			return result == null ? new List<LookupModel>() : result.ToList();
		}

		public async Task<bool> AlreadySeenData(string objectId)
		{
			var result = (await _lookupLogic.Get(_user.AccountId, _user.InstagramAccountUser))
				?.Where(_=> _.ObjectId == objectId 
						&& _.LookupStatus == LookupStatus.Completed
						&& _.ActionType == _actionType);

			return result != null && result.Any();
		}
	}
}