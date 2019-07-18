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
		
		FetchMediaByUserTargetList,
		FetchMediaByUserLocationTargetList,
		
		FetchMediaByLikers,
		FetchMediaByCommenters,
		FetchUsersViaPostLiked,
		FetchUsersViaPostCommented,

		FetchCommentsViaPostCommented,
		FetchCommentsViaPostsLiked,
		FetchCommentsViaUserTargetList,
		FetchCommentsViaLocationTargetList,
		FetchCommentsViaUserFeed
	}
}
