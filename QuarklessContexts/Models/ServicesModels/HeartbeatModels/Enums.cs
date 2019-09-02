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
		FetchUsersFollowerList,

		FetchUsersFeed,
		FetchMediaByTopic,
		FetchMediaByTopicRecent,

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
