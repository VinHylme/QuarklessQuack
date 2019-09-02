using System;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;

namespace QuarklessLogic.Logic.MessagingLogic
{
	public class MessagingLogic : IMessagingLogic
	{
		private IReportHandler _reportHandler { get; set; }
		private readonly IAPIClientContainer _client;
		public MessagingLogic(IReportHandler reportHandler, IAPIClientContainer client)
		{
			_reportHandler = reportHandler;
			_client = client;
			var account = _client?.GetContext?.InstagramAccount;
			if(account!=null)
				_reportHandler.SetupReportHandler(nameof(MessagingLogic), account.AccountId, account.Id);
			else
			{
				_reportHandler.SetupReportHandler(nameof(MessagingLogic));
			}
		}

		public async Task<IResult<InstaDirectInboxThread>> AddUserToGroupThreadAsync(string threadId, params long[] userIds)
		{
			try
			{
				return await _client.Messaging.AddUserToGroupThreadAsync(threadId, userIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> ApproveDirectPendingRequestAsync(params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.ApproveDirectPendingRequestAsync(threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> DeclineAllDirectPendingRequestsAsync()
		{
			try
			{
				return await _client.Messaging.DeclineAllDirectPendingRequestsAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> DeclineDirectPendingRequestsAsync(params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.DeclineDirectPendingRequestsAsync(threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> DeleteDirectThreadAsync(string threadId)
		{
			try
			{
				return await _client.Messaging.DeleteDirectThreadAsync(threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> DeleteSelfMessageAsync(string threadId, string itemId)
		{
			try
			{
				return await _client.Messaging.DeleteSelfMessageAsync(threadId, itemId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaDirectInboxContainer>> GetDirectInboxAsync(int limit)
		{
			try
			{
				return await _client.Messaging.GetDirectInboxAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaDirectInboxThread>> GetDirectInboxThreadAsync(string threadId, int limit)
		{
			try
			{
				return await _client.Messaging.GetDirectInboxThreadAsync(threadId, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaDirectInboxContainer>> GetPendingDirectAsync(int limit)
		{
			try
			{
				return await _client.Messaging.GetPendingDirectAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaRecipients>> GetRankedRecipientsAsync()
		{
			try
			{
				return await _client.Messaging.GetRankedRecipientsAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaRecipients>> GetRankedRecipientsByUsernameAsync(string username)
		{
			try
			{
				return await _client.Messaging.GetRankedRecipientsByUsernameAsync(username);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaRecipients>> GetRecentRecipientsAsync()
		{
			try
			{
				return await _client.Messaging.GetRankedRecipientsAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaUserPresenceList>> GetUsersPresenceAsync()
		{
			try
			{
				return await _client.Messaging.GetUsersPresenceAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> LeaveGroupThreadAsync(string threadId)
		{
			try
			{
				return await _client.Messaging.LeaveGroupThreadAsync(threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> LikeThreadMessageAsync(string threadId, string itemId)
		{
			try
			{
				return await _client.Messaging.LikeThreadMessageAsync(threadId, itemId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> MarkDirectThreadAsSeenAsync(string threadId, string itemId)
		{
			try
			{
				return await _client.Messaging.MarkDirectThreadAsSeenAsync(threadId, itemId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> MuteDirectThreadMessagesAsync(string threadId)
		{
			try
			{
				return await _client.Messaging.MuteDirectThreadMessagesAsync(threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> MuteDirectThreadVideoCallsAsync(string threadId)
		{
			try
			{
				return await _client.Messaging.MuteDirectThreadVideoCallsAsync(threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaDirectInboxThread>> SendDirectAnimatedMediaAsync(string giphyId, params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.SendDirectAnimatedMediaAsync(giphyId, threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaDirectInboxThread>> SendDirectAnimatedMediaToRecipientsAsync(string giphyId, params string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectAnimatedMediaToRecipientsAsync(giphyId, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectDisappearingPhotoAsync(InstaImage image, InstaViewMode viewMode = InstaViewMode.Replayable,
			params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.SendDirectDisappearingPhotoAsync(image, viewMode, threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectDisappearingPhotoAsync(Action<InstaUploaderProgress> progress, InstaImage image,
			InstaViewMode viewMode = InstaViewMode.Replayable, params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.SendDirectDisappearingPhotoAsync(progress, image, viewMode, threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectDisappearingVideoAsync(InstaVideoUpload video, InstaViewMode viewMode = InstaViewMode.Replayable,
			params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.SendDirectDisappearingVideoAsync(video, viewMode, threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectDisappearingVideoAsync(Action<InstaUploaderProgress> progress, InstaVideoUpload video,
			InstaViewMode viewMode = InstaViewMode.Replayable, params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.SendDirectDisappearingVideoAsync(progress, video, viewMode, threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectFelixShareAsync(string mediaId, string[] threadIds, string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectFelixShareAsync(mediaId, threadIds, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectHashtagAsync(string text, string hashtag, params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.SendDirectHashtagAsync(text, hashtag, threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectHashtagAsync(string text, string hashtag, string[] threadIds, string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectHashtagAsync(text, hashtag, threadIds, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectHashtagToRecipientsAsync(string text, string hashtag, params string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectHashtagToRecipientsAsync(text, hashtag, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectLinkAsync(string text, string link, params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.SendDirectLinkAsync(text, link , threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectLinkAsync(string text, string link, string[] threadIds, string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectLinkAsync(text, link, threadIds, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectLinkToRecipientsAsync(string text, string link, params string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectLinkToRecipientsAsync(text, link, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectLocationAsync(string externalId, params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.SendDirectLocationAsync(externalId, threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectPhotoAsync(InstaImage image, string threadId)
		{
			try
			{
				return await _client.Messaging.SendDirectPhotoAsync(image, threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectPhotoAsync(Action<InstaUploaderProgress> progress, InstaImage image, string threadId)
		{
			try
			{
				return await _client.Messaging.SendDirectPhotoAsync(progress, image, threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectPhotoToRecipientsAsync(InstaImage image, params string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectPhotoToRecipientsAsync(image, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectPhotoToRecipientsAsync(Action<InstaUploaderProgress> progress, InstaImage image, params string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectPhotoToRecipientsAsync(progress, image, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectProfileAsync(long userIdToSend, params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.SendDirectProfileAsync(userIdToSend, threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectProfileToRecipientsAsync(long userIdToSend, string recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectProfileToRecipientsAsync(userIdToSend, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaDirectRespondPayload>> SendDirectTextAsync(string recipients, string threadIds, string text)
		{
			try
			{
				return await _client.Messaging.SendDirectTextAsync(recipients, threadIds, text);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectVideoAsync(InstaVideoUpload video, string threadId)
		{
			try
			{
				return await _client.Messaging.SendDirectVideoAsync(video, threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectVideoAsync(Action<InstaUploaderProgress> progress, InstaVideoUpload video, string threadId)
		{
			try
			{
				return await _client.Messaging.SendDirectVideoAsync(progress, video, threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectVideoToRecipientsAsync(InstaVideoUpload video, params string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectVideoToRecipientsAsync(video, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectVideoToRecipientsAsync(Action<InstaUploaderProgress> progress, InstaVideoUpload video, params string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectVideoToRecipientsAsync(progress, video, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectVoiceAsync(InstaAudioUpload audio, string threadId)
		{
			try
			{
				return await _client.Messaging.SendDirectVoiceAsync(audio, threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectVoiceAsync(Action<InstaUploaderProgress> progress, InstaAudioUpload audio, string threadId)
		{
			try
			{
				return await _client.Messaging.SendDirectVoiceAsync(progress, audio, threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectVoiceToRecipientsAsync(InstaAudioUpload audio, params string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectVoiceToRecipientsAsync(audio, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectVoiceToRecipientsAsync(Action<InstaUploaderProgress> progress, InstaAudioUpload audio, params string[] recipients)
		{
			try
			{
				return await _client.Messaging.SendDirectVoiceToRecipientsAsync(progress, audio, recipients);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> ShareMediaToThreadAsync(string mediaId, InstaMediaType mediaType, string text, params string[] threadIds)
		{
			try
			{
				return await _client.Messaging.ShareMediaToThreadAsync(mediaId, mediaType, text, threadIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> ShareMediaToUserAsync(string mediaId, InstaMediaType mediaType, string text, params long[] userIds)
		{
			try
			{
				return await _client.Messaging.ShareMediaToUserAsync(mediaId, mediaType, text, userIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> UnLikeThreadMessageAsync(string threadId, string itemId)
		{
			try
			{
				return await _client.Messaging.UnLikeThreadMessageAsync(threadId, itemId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> UnMuteDirectThreadMessagesAsync(string threadId)
		{
			try
			{
				return await _client.Messaging.UnMuteDirectThreadMessagesAsync(threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> UnMuteDirectThreadVideoCallsAsync(string threadId)
		{
			try
			{
				return await _client.Messaging.UnMuteDirectThreadVideoCallsAsync(threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> UpdateDirectThreadTitleAsync(string threadId, string title)
		{
			try
			{
				return await _client.Messaging.UpdateDirectThreadTitleAsync(threadId, title);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> SendDirectLikeAsync(string threadId)
		{
			try
			{
				return await _client.Messaging.SendDirectLikeAsync(threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
	}
}
