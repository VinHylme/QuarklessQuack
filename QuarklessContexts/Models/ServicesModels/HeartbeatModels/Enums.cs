namespace QuarklessContexts.Models.ServicesModels.HeartbeatModels
{
	public enum MetaDataType
	{
		None,
		FetchMediaForSpecificUserGoogle,
		FetchMediaForSpecificUserYandexQuery,
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
		FetchCommentsViaUserFeed,

		FetchUserDirectInbox,
		UsersRecentComments
	}
}
