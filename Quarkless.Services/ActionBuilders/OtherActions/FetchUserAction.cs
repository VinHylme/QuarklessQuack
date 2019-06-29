using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.ActionBuilders.OtherActions
{
	public enum UserFetchType
	{
		UserFollowingList
	}
	public class UserFetchResponse : IFetchResponse
	{
		public object FetchedItems { get; set; }
	}
	class FetchUserAction : IFetched
	{
		private readonly ProfileModel _profile;
		private readonly IContentManager _builder;
		public FetchUserAction(IContentManager contentBuild, ProfileModel profile)
		{
			_profile = profile;
			_builder = contentBuild;
		}

		public IFetchResponse FetchByTopic(int totalTopics = 15, int takeAmount = 2)
		{
			throw new NotImplementedException();
		}

		public IFetchResponse FetchUsers(int limit, UserFetchType userFetchType)
		{
			switch (userFetchType)
			{
				case UserFetchType.UserFollowingList:
					var users = _builder.GetUserFollowingList(_profile.InstagramAccountId,limit);
					return new UserFetchResponse
					{
						FetchedItems = users
					};
			}

			return null;
		}
	}
}
