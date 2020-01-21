using InstagramApiSharp.Classes;
using InstagramApiSharp.Enums;
using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;

namespace Quarkless.Base.AccountOptions
{
	public interface IInstaAccountOptionsLogic
	{
		Task<IResult<bool>> AllowStoryMessageRepliesAsync(InstaMessageRepliesType repliesType);
		Task<IResult<bool>> AllowStorySharingAsync(bool allow = true);
		Task<IResult<bool>> ChangePasswordAsync(string oldPassword, string newPassword);
		Task<IResult<InstaUserEdit>> ChangeProfilePictureAsync(Action<InstaUploaderProgress> progress, byte[] pictureBytes);
		Task<IResult<InstaUserEdit>> ChangeProfilePictureAsync(byte[] pictureBytes);
		Task<IResult<InstaAccountCheck>> CheckUsernameAsync(string desiredUsername);
		Task<IResult<bool>> DisablePresenceAsync();
		Task<IResult<bool>> DisableSaveStoryToArchiveAsync();
		Task<IResult<bool>> DisableSaveStoryToGalleryAsync();
		Task<IResult<bool>> DisableTwoFactorAuthenticationAsync();
		Task<IResult<InstaUserEdit>> EditProfileAsync(string name, string biography, string url, string email, string phone, InstaGenderType? gender, string newUsername = null);
		Task<IResult<bool>> EnablePresenceAsync();
		Task<IResult<bool>> EnableSaveStoryToArchiveAsync();
		Task<IResult<bool>> EnableSaveStoryToGalleryAsync();
		Task<IResult<InstaPresence>> GetPresenceOptionsAsync();
		Task<IResult<InstaRequestDownloadData>> GetRequestForDownloadAccountDataAsync(string email);
		Task<IResult<InstaRequestDownloadData>> GetRequestForDownloadAccountDataAsync(string email, string password);
		Task<IResult<InstaUserEdit>> GetRequestForEditProfileAsync();
		Task<IResult<InstaAccountSecuritySettings>> GetSecuritySettingsInfoAsync();
		Task<IResult<InstaStorySettings>> GetStorySettingsAsync();
		Task<IResult<TwoFactorRegenBackupCodes>> RegenerateTwoFactorBackupCodesAsync();
		Task<IResult<InstaUserEdit>> RemoveProfilePictureAsync();
		Task<IResult<InstaAccountConfirmEmail>> SendConfirmEmailAsync();
		Task<IResult<InstaAccountSendSms>> SendSmsCodeAsync(string phoneNumber);
		Task<IResult<InstaAccountTwoFactorSms>> SendTwoFactorEnableSmsAsync(string phoneNumber);
		Task<IResult<InstaUserShort>> SetAccountPrivateAsync();
		Task<IResult<InstaUserShort>> SetAccountPublicAsync();
		Task<IResult<InstaBiography>> SetBiographyAsync(string bio);
		Task<IResult<bool>> SetNameAndPhoneNumberAsync(string name, string phoneNumber = "");
		Task<IResult<InstaBusinessUser>> SwitchToBusinessAccountAsync();
		Task<IResult<InstaUser>> SwitchToPersonalAccountAsync();
		Task<IResult<InstaAccountTwoFactor>> TwoFactorEnableAsync(string phoneNumber, string verificationCode);
		Task<IResult<InstaMedia>> UploadNametagAsync(InstaImage nametagImage);
		Task<IResult<bool>> VerifyEmailByVerificationUriAsync(Uri verificationUri);
		Task<IResult<InstaAccountVerifySms>> VerifySmsCodeAsync(string phoneNumber, string verificationCode);
	}
}