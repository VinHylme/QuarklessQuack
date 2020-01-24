using Quarkless.SmsHandler.Models.Enums;
using System.Collections.Generic;

namespace Quarkless.SmsHandler.Logic
{
	public static class SmsFactory
	{
		private static readonly IDictionary<SmsServiceType, SmsServiceCreator> _serviceCreators;
		static SmsFactory()
		{
			_serviceCreators = new Dictionary<SmsServiceType, SmsServiceCreator>
			{
				{ SmsServiceType.Google, new GoogleSmsCreator() },
				{ SmsServiceType.Instagram, new InstagramSmsCreator() }
			};
		}

		public static ISmsService Create(SmsServiceType type = SmsServiceType.Google, 
			CountryCode code = CountryCode.UnitedKingdom)
			=> _serviceCreators[type].Create(code);
	}
}