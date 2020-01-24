using Quarkless.SmsHandler.Models;
using Quarkless.SmsHandler.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.SmsHandler.Logic
{
	public class GoogleSmsService : ISmsService
	{
		private const int MAX_NUMBER_USAGE = 3;
		private int _positionOfMessage;

		private static readonly object Locker = new object();
		private static readonly IDictionary<CountryCode, int> TotalInstancesPerNumber = new Dictionary<CountryCode, int>();
		private static bool _isCurrentNumberBad;

		private readonly SmsService _smsService;
		private readonly CountryCode _countryCode;
		public GoogleSmsService(CountryCode countryCode)
		{
			_countryCode = countryCode;

			if (!TotalInstancesPerNumber.ContainsKey(_countryCode))
				TotalInstancesPerNumber.Add(_countryCode, 0);

			_smsService = new SmsService(SmsServiceType.Google, _countryCode);

			lock (Locker)
			{
				_positionOfMessage += TotalInstancesPerNumber[_countryCode];
				TotalInstancesPerNumber[_countryCode]++;
			}
		}

		public async Task<Status> GetVerificationCode(int tZid)
		{
			var response = await _smsService.GetVerificationCode(tZid, _positionOfMessage);
			if (response == null)
				_isCurrentNumberBad = true;
			return response;
		}

		public async Task<Status> IssueNumber()
		{
			Status issue;

			if (_isCurrentNumberBad)
			{
				issue = await _smsService.IssueNumber(true);
				_isCurrentNumberBad = false;
				if (issue == null)
					return null;
			}
			else
			{
				if (TotalInstancesPerNumber[_countryCode] > MAX_NUMBER_USAGE)
				{
					issue = await _smsService.IssueNumber(true);
					if (issue == null)
						return null;
				}
				else
				{
					issue = await _smsService.IssueNumber();
					if (issue == null)
						return null;
				}
			}

			if (issue.IsNewNumber && TotalInstancesPerNumber[_countryCode] > 1)
			{
				TotalInstancesPerNumber[_countryCode] = 1;
				_positionOfMessage = 0;
			}

			if (TotalInstancesPerNumber[_countryCode] == 1 && issue.MessageCount > 0)
			{
				TotalInstancesPerNumber[_countryCode] = issue.MessageCount + 1;
				_positionOfMessage = issue.MessageCount;
			}
			return issue;
		}

		public Task<Status> IssueNumberNew()
		{
			return _smsService.IssueNumber(true);
		}
	}
}