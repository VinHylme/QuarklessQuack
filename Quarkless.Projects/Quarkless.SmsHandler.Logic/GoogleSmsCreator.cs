using Quarkless.SmsHandler.Models.Enums;

namespace Quarkless.SmsHandler.Logic
{
	public class GoogleSmsCreator : SmsServiceCreator
	{
		public override ISmsService Create(CountryCode code)
			=> new GoogleSmsService(code);
	}
}