using Quarkless.SmsHandler.Models.Enums;

namespace Quarkless.SmsHandler.Logic
{
	public class InstagramSmsCreator : SmsServiceCreator
	{
		public override ISmsService Create(CountryCode code)
			=> new InstagramSmsService(code);
	}
}