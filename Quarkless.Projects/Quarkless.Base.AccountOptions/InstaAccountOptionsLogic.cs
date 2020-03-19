using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.ReportHandler.Models.Interfaces;

namespace Quarkless.Base.AccountOptions
{
	public class InstaAccountOptionsLogic : IInstaAccountOptionsLogic
	{
		private readonly IApiClientContainer _client;
		private readonly IReportHandler _reportHandler;

		public InstaAccountOptionsLogic(IApiClientContainer client, IReportHandler reportHandler)
		{
			this._client = client;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("Logic/InstaAccountOptions");
		}
		public async Task<IResult<bool>> AllowStoryMessageRepliesAsync(InstaMessageRepliesType repliesType)
		{
			try
			{
				return await _client.Account.AllowStoryMessageRepliesAsync(repliesType);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> AllowStorySharingAsync(bool allow = true)
		{
			try
			{
				return await _client.Account.AllowStorySharingAsync(allow);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> ChangePasswordAsync(string oldPassword, string newPassword)
		{
			try
			{
				return await _client.Account.ChangePasswordAsync(oldPassword, newPassword);
			}
			catch (Exception ee)
			{ 
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserEdit>> ChangeProfilePictureAsync(byte[] pictureBytes)
		{
			try
			{
				return await _client.Account.ChangeProfilePictureAsync(pictureBytes);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserEdit>> ChangeProfilePictureAsync(Action<InstaUploaderProgress> progress, byte[] pictureBytes)
		{
			try
			{
				return await _client.Account.ChangeProfilePictureAsync(pictureBytes);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountCheck>> CheckUsernameAsync(string desiredUsername)
		{
			try
			{
				return await _client.Account.CheckUsernameAsync(desiredUsername);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> DisablePresenceAsync()
		{
			try
			{
				return await _client.Account.DisablePresenceAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> DisableSaveStoryToArchiveAsync()
		{
			try
			{
				return await _client.Account.DisableSaveStoryToArchiveAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> DisableSaveStoryToGalleryAsync()
		{
			try
			{
				return await _client.Account.DisableSaveStoryToGalleryAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> DisableTwoFactorAuthenticationAsync()
		{
			try
			{
				return await _client.Account.DisableTwoFactorAuthenticationAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserEdit>> EditProfileAsync(string name, string biography, string url, string email, string phone, InstaGenderType? gender, string newUsername = null)
		{
			try
			{
				return await _client.Account.EditProfileAsync(name, biography, url, email, phone, gender, newUsername);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> EnablePresenceAsync()
		{
			try
			{
				return await _client.Account.EnablePresenceAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> EnableSaveStoryToArchiveAsync()
		{
			try
			{
				return await _client.Account.EnableSaveStoryToArchiveAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> EnableSaveStoryToGalleryAsync()
		{
			try
			{
				return await _client.Account.EnableSaveStoryToGalleryAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaPresence>> GetPresenceOptionsAsync()
		{
			try
			{
				return await _client.Account.GetPresenceOptionsAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaRequestDownloadData>> GetRequestForDownloadAccountDataAsync(string email)
		{
			try
			{
				return await _client.Account.GetRequestForDownloadAccountDataAsync(email);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaRequestDownloadData>> GetRequestForDownloadAccountDataAsync(string email, string password)
		{
			try
			{
				return await _client.Account.GetRequestForDownloadAccountDataAsync(email, password);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserEdit>> GetRequestForEditProfileAsync()
		{
			try
			{
				return await _client.Account.GetRequestForEditProfileAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountSecuritySettings>> GetSecuritySettingsInfoAsync()
		{
			try
			{
				return await _client.Account.GetSecuritySettingsInfoAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaStorySettings>> GetStorySettingsAsync()
		{
			try
			{
				return await _client.Account.GetStorySettingsAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<TwoFactorRegenBackupCodes>> RegenerateTwoFactorBackupCodesAsync()
		{
			try
			{
				return await _client.Account.RegenerateTwoFactorBackupCodesAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserEdit>> RemoveProfilePictureAsync()
		{
			try
			{
				return await _client.Account.RemoveProfilePictureAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountConfirmEmail>> SendConfirmEmailAsync()
		{
			try
			{
				return await _client.Account.SendConfirmEmailAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountSendSms>> SendSmsCodeAsync(string phoneNumber)
		{
			try
			{
				return await _client.Account.SendSmsCodeAsync(phoneNumber);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountTwoFactorSms>> SendTwoFactorEnableSmsAsync(string phoneNumber)
		{
			try
			{
				return await _client.Account.SendTwoFactorEnableSmsAsync(phoneNumber);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserShort>> SetAccountPrivateAsync()
		{
			try
			{
				return await _client.Account.SetAccountPrivateAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserShort>> SetAccountPublicAsync()
		{
			try
			{
				return await _client.Account.SetAccountPublicAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaBiography>> SetBiographyAsync(string bio)
		{
			try
			{
				if(_client.GetContext.Container.InstagramAccount.UserBiography.Text == bio) 
					return new Result<InstaBiography>(true, 
						new InstaBiography()
						{
							Status = "Done",
							User = new InstaBiographyUser
							{
								Biography = bio
							}
						});
				return await _client.Account.SetBiographyAsync(bio);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> SetNameAndPhoneNumberAsync(string name, string phoneNumber = "")
		{
			try
			{
				return await _client.Account.SetNameAndPhoneNumberAsync(name, phoneNumber);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaBusinessUser>> SwitchToBusinessAccountAsync()
		{
			try
			{
				return await _client.Account.SwitchToBusinessAccountAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUser>> SwitchToPersonalAccountAsync()
		{
			try
			{
				return await _client.Account.SwitchToPersonalAccountAsync();
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountTwoFactor>> TwoFactorEnableAsync(string phoneNumber, string verificationCode)
		{
			try
			{
				return await _client.Account.TwoFactorEnableAsync(phoneNumber, verificationCode);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaMedia>> UploadNametagAsync(InstaImage nametagImage)
		{
			try
			{
				return await _client.Account.UploadNametagAsync(nametagImage);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> VerifyEmailByVerificationUriAsync(Uri verificationUri)
		{
			try
			{
				return await _client.Account.VerifyEmailByVerificationUriAsync(verificationUri);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountVerifySms>> VerifySmsCodeAsync(string phoneNumber, string verificationCode)
		{
			try
			{
				return await _client.Account.VerifySmsCodeAsync(phoneNumber, verificationCode);
			}
			catch (Exception ee)
			{
				await _reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
	}
}
