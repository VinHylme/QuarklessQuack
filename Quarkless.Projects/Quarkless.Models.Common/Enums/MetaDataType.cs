namespace Quarkless.Models.Common.Enums
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
		FetchUsersStoryFeed,
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
		UsersRecentComments,
		FetchUsersStoryViaTopics
	}
}