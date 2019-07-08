using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.HeartbeatModels
{
	public enum MetaDataType
	{
		None,
		FetchMediaForSpecificUserGoogle,
		FetchMediaForSepcificUserYandexQuery,
		FetchMediaForSpecificUserYandex,
		FetchUserOwnProfile,
		FetchUsersFollowingList,
		FetchUsersFollowSuggestions,
		FetchUsersFeed,
		FetchMediaByTopic,
		FetchMediaByLikers,
		FetchMediaByCommenters,
		FetchUsersViaPostLiked,
		FetchUsersViaPostCommented
	}
}
