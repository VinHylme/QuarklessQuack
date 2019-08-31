using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using System;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.InstaAccountOptionsLogic
{
	public class InstaAccountOptionsLogic : IInstaAccountOptionsLogic
	{
		private readonly IAPIClientContainer _Client;
		private readonly IReportHandler _reportHandler;

		public InstaAccountOptionsLogic(IAPIClientContainer client, IReportHandler reportHandler)
		{
			this._Client = client;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("Logic/InstaAccountOptions");
		}
		public async Task<IResult<bool>> AllowStoryMessageRepliesAsync(InstaMessageRepliesType repliesType)
		{
			try
			{
				return await _Client.Account.AllowStoryMessageRepliesAsync(repliesType);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> AllowStorySharingAsync(bool allow = true)
		{
			try
			{
				return await _Client.Account.AllowStorySharingAsync(allow);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> ChangePasswordAsync(string oldPassword, string newPassword)
		{
			try
			{
				return await _Client.Account.ChangePasswordAsync(oldPassword, newPassword);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserEdit>> ChangeProfilePictureAsync(byte[] pictureBytes)
		{
			try
			{
				return await _Client.Account.ChangeProfilePictureAsync(pictureBytes);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserEdit>> ChangeProfilePictureAsync(Action<InstaUploaderProgress> progress, byte[] pictureBytes)
		{
			try
			{
				return await _Client.Account.ChangeProfilePictureAsync(pictureBytes);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountCheck>> CheckUsernameAsync(string desiredUsername)
		{
			try
			{
				return await _Client.Account.CheckUsernameAsync(desiredUsername);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> DisablePresenceAsync()
		{
			try
			{
				return await _Client.Account.DisablePresenceAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> DisableSaveStoryToArchiveAsync()
		{
			try
			{
				return await _Client.Account.DisableSaveStoryToArchiveAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> DisableSaveStoryToGalleryAsync()
		{
			try
			{
				return await _Client.Account.DisableSaveStoryToGalleryAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> DisableTwoFactorAuthenticationAsync()
		{
			try
			{
				return await _Client.Account.DisableTwoFactorAuthenticationAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserEdit>> EditProfileAsync(string name, string biography, string url, string email, string phone, InstaGenderType? gender, string newUsername = null)
		{
			try
			{
				return await _Client.Account.EditProfileAsync(name, biography, url, email, phone, gender, newUsername);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> EnablePresenceAsync()
		{
			try
			{
				return await _Client.Account.EnablePresenceAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> EnableSaveStoryToArchiveAsync()
		{
			try
			{
				return await _Client.Account.EnableSaveStoryToArchiveAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> EnableSaveStoryToGalleryAsync()
		{
			try
			{
				return await _Client.Account.EnableSaveStoryToGalleryAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaPresence>> GetPresenceOptionsAsync()
		{
			try
			{
				return await _Client.Account.GetPresenceOptionsAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaRequestDownloadData>> GetRequestForDownloadAccountDataAsync(string email)
		{
			try
			{
				return await _Client.Account.GetRequestForDownloadAccountDataAsync(email);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaRequestDownloadData>> GetRequestForDownloadAccountDataAsync(string email, string password)
		{
			try
			{
				return await _Client.Account.GetRequestForDownloadAccountDataAsync(email, password);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserEdit>> GetRequestForEditProfileAsync()
		{
			try
			{
				return await _Client.Account.GetRequestForEditProfileAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountSecuritySettings>> GetSecuritySettingsInfoAsync()
		{
			try
			{
				return await _Client.Account.GetSecuritySettingsInfoAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaStorySettings>> GetStorySettingsAsync()
		{
			try
			{
				return await _Client.Account.GetStorySettingsAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<TwoFactorRegenBackupCodes>> RegenerateTwoFactorBackupCodesAsync()
		{
			try
			{
				return await _Client.Account.RegenerateTwoFactorBackupCodesAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserEdit>> RemoveProfilePictureAsync()
		{
			try
			{
				return await _Client.Account.RemoveProfilePictureAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountConfirmEmail>> SendConfirmEmailAsync()
		{
			try
			{
				return await _Client.Account.SendConfirmEmailAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountSendSms>> SendSmsCodeAsync(string phoneNumber)
		{
			try
			{
				return await _Client.Account.SendSmsCodeAsync(phoneNumber);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountTwoFactorSms>> SendTwoFactorEnableSmsAsync(string phoneNumber)
		{
			try
			{
				return await _Client.Account.SendTwoFactorEnableSmsAsync(phoneNumber);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserShort>> SetAccountPrivateAsync()
		{
			try
			{
				return await _Client.Account.SetAccountPrivateAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUserShort>> SetAccountPublicAsync()
		{
			try
			{
				return await _Client.Account.SetAccountPublicAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaBiography>> SetBiographyAsync(string bio)
		{
			try
			{
				if(_Client.GetContext.InstagramAccount.UserBiography.Text == bio) 
					return new Result<InstaBiography>(true, 
						new InstaBiography()
						{
							Status = "Done",
							User = new InstaBiographyUser
							{
								Biography = bio
							}
						});
				return await _Client.Account.SetBiographyAsync(bio);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> SetNameAndPhoneNumberAsync(string name, string phoneNumber = "")
		{
			try
			{
				return await _Client.Account.SetNameAndPhoneNumberAsync(name, phoneNumber);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaBusinessUser>> SwitchToBusinessAccountAsync()
		{
			try
			{
				return await _Client.Account.SwitchToBusinessAccountAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaUser>> SwitchToPersonalAccountAsync()
		{
			try
			{
				return await _Client.Account.SwitchToPersonalAccountAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountTwoFactor>> TwoFactorEnableAsync(string phoneNumber, string verificationCode)
		{
			try
			{
				return await _Client.Account.TwoFactorEnableAsync(phoneNumber, verificationCode);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaMedia>> UploadNametagAsync(InstaImage nametagImage)
		{
			try
			{
				return await _Client.Account.UploadNametagAsync(nametagImage);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<bool>> VerifyEmailByVerificationUriAsync(Uri verificationUri)
		{
			try
			{
				return await _Client.Account.VerifyEmailByVerificationUriAsync(verificationUri);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

		public async Task<IResult<InstaAccountVerifySms>> VerifySmsCodeAsync(string phoneNumber, string verificationCode)
		{
			try
			{
				return await _Client.Account.VerifySmsCodeAsync(phoneNumber, verificationCode);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
	}
}
