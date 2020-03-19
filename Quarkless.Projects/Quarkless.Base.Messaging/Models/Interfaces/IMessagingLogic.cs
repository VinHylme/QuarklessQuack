using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;

namespace Quarkless.Base.Messaging.Models.Interfaces
{
	public interface IMessagingLogic
	{
		Task<IResult<InstaDirectInboxThread>> AddUserToGroupThreadAsync(string threadId, params long[] userIds);
		Task<IResult<bool>> ApproveDirectPendingRequestAsync(params string[] threadIds);
		Task<IResult<bool>> DeclineAllDirectPendingRequestsAsync();
		Task<IResult<bool>> DeclineDirectPendingRequestsAsync(params string[] threadIds);
		Task<IResult<bool>> DeleteDirectThreadAsync(string threadId);
		Task<IResult<bool>> DeleteSelfMessageAsync(string threadId, string itemId);
		Task<IResult<InstaDirectInboxContainer>> GetDirectInboxAsync(int limit);
		Task<IResult<InstaDirectInboxThread>> GetDirectInboxThreadAsync(string threadId, int limit);
		Task<IResult<InstaDirectInboxContainer>> GetPendingDirectAsync(int limit);
		Task<IResult<InstaRecipients>> GetRankedRecipientsAsync();
		Task<IResult<InstaRecipients>> GetRankedRecipientsByUsernameAsync(string username);
		Task<IResult<InstaRecipients>> GetRecentRecipientsAsync();
		Task<IResult<InstaUserPresenceList>> GetUsersPresenceAsync();
		Task<IResult<bool>> LeaveGroupThreadAsync(string threadId);
		Task<IResult<bool>> LikeThreadMessageAsync(string threadId, string itemId);
		Task<IResult<bool>> MarkDirectThreadAsSeenAsync(string threadId, string itemId);
		Task<IResult<bool>> MuteDirectThreadMessagesAsync(string threadId);
		Task<IResult<bool>> MuteDirectThreadVideoCallsAsync(string threadId);
		Task<IResult<InstaDirectInboxThread>> SendDirectAnimatedMediaAsync(string giphyId, params string[] threadIds);
		Task<IResult<InstaDirectInboxThread>> SendDirectAnimatedMediaToRecipientsAsync(string giphyId, params string[] recipients);

		Task<IResult<bool>> SendDirectDisappearingPhotoAsync(InstaImage image, InstaViewMode viewMode = InstaViewMode.Replayable,
			params string[] threadIds);

		Task<IResult<bool>> SendDirectDisappearingPhotoAsync(Action<InstaUploaderProgress> progress, InstaImage image,
			InstaViewMode viewMode = InstaViewMode.Replayable, params string[] threadIds);

		Task<IResult<bool>> SendDirectDisappearingVideoAsync(InstaVideoUpload video, InstaViewMode viewMode = InstaViewMode.Replayable,
			params string[] threadIds);

		Task<IResult<bool>> SendDirectDisappearingVideoAsync(Action<InstaUploaderProgress> progress, InstaVideoUpload video,
			InstaViewMode viewMode = InstaViewMode.Replayable, params string[] threadIds);

		Task<IResult<bool>> SendDirectFelixShareAsync(string mediaId, string[] threadIds, string[] recipients);
		Task<IResult<bool>> SendDirectHashtagAsync(string text, string hashtag, params string[] threadIds);
		Task<IResult<bool>> SendDirectHashtagAsync(string text, string hashtag, string[] threadIds, string[] recipients);
		Task<IResult<bool>> SendDirectHashtagToRecipientsAsync(string text, string hashtag, params string[] recipients);
		Task<IResult<InstaDirectRespondPayload>> SendDirectLinkAsync(string text, string link, params string[] threadIds);
		Task<IResult<InstaDirectRespondPayload>> SendDirectLinkAsync(string text, string link, string[] threadIds, string[] recipients);
		Task<IResult<InstaDirectRespondPayload>> SendDirectLinkToRecipientsAsync(string text, string link, params string[] recipients);
		Task<IResult<bool>> SendDirectLocationAsync(string externalId, params string[] threadIds);
		Task<IResult<bool>> SendDirectPhotoAsync(InstaImage image, string threadId);
		Task<IResult<bool>> SendDirectPhotoAsync(Action<InstaUploaderProgress> progress, InstaImage image, string threadId);
		Task<IResult<bool>> SendDirectPhotoToRecipientsAsync(InstaImage image, params string[] recipients);
		Task<IResult<bool>> SendDirectPhotoToRecipientsAsync(Action<InstaUploaderProgress> progress, InstaImage image, params string[] recipients);
		Task<IResult<bool>> SendDirectProfileAsync(long userIdToSend, params string[] threadIds);
		Task<IResult<bool>> SendDirectProfileToRecipientsAsync(long userIdToSend, string recipients);
		Task<IResult<InstaDirectRespondPayload>> SendDirectTextAsync(string recipients, string threadIds, string text);
		Task<IResult<bool>> SendDirectVideoAsync(InstaVideoUpload video, string threadId);
		Task<IResult<bool>> SendDirectVideoAsync(Action<InstaUploaderProgress> progress, InstaVideoUpload video, string threadId);
		Task<IResult<bool>> SendDirectVideoToRecipientsAsync(InstaVideoUpload video, params string[] recipients);
		Task<IResult<bool>> SendDirectVideoToRecipientsAsync(Action<InstaUploaderProgress> progress, InstaVideoUpload video, params string[] recipients);
		Task<IResult<bool>> SendDirectVoiceAsync(InstaAudioUpload audio, string threadId);
		Task<IResult<bool>> SendDirectVoiceAsync(Action<InstaUploaderProgress> progress, InstaAudioUpload audio, string threadId);
		Task<IResult<bool>> SendDirectVoiceToRecipientsAsync(InstaAudioUpload audio, params string[] recipients);
		Task<IResult<bool>> SendDirectVoiceToRecipientsAsync(Action<InstaUploaderProgress> progress, InstaAudioUpload audio, params string[] recipients);
		Task<IResult<bool>> ShareMediaToThreadAsync(string mediaId, InstaMediaType mediaType, string text, params string[] threadIds);
		Task<IResult<bool>> ShareMediaToUserAsync(string mediaId, InstaMediaType mediaType, string text, params long[] userIds);
		Task<IResult<bool>> UnLikeThreadMessageAsync(string threadId, string itemId);
		Task<IResult<bool>> UnMuteDirectThreadMessagesAsync(string threadId);
		Task<IResult<bool>> UnMuteDirectThreadVideoCallsAsync(string threadId);
		Task<IResult<bool>> UpdateDirectThreadTitleAsync(string threadId, string title);
		Task<IResult<bool>> SendDirectLikeAsync(string threadId);
	}
}