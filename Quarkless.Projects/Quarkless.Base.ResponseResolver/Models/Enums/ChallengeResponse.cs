namespace Quarkless.Base.ResponseResolver.Models.Enums
{
	public enum ChallengeResponse
	{
		RequirePhoneNumber,
		RequireSmsCode,
		RequireEmailCode,
		Ok,
		Captcha,
		Unknown,
		None
	}
}
