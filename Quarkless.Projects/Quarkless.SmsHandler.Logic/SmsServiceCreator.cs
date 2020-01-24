using Quarkless.SmsHandler.Models.Enums;

namespace Quarkless.SmsHandler.Logic
{
	public abstract class SmsServiceCreator
	{
		public abstract ISmsService Create(CountryCode code);
	}
}