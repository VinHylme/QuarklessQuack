namespace Quarkless.Models.ResponseResolver.Enums
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
