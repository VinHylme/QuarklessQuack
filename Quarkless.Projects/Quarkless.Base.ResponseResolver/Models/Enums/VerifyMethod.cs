using System.ComponentModel;

namespace Quarkless.Base.ResponseResolver.Models.Enums
{
	public enum VerifyMethod
	{
		[Description("captcha")]
		Captcha,
		[Description("phone_number")]
		Phone,
		[Description("email_code")]
		Email,
		[Description("sms_code")]
		Sms,
		[Description("none")]
		None
	}
}