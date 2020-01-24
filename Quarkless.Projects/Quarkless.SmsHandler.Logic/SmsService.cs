using Quarkless.SmsHandler.Models;
using Quarkless.SmsHandler.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Quarkless.SmsHandler.Logic
{
	public class SmsService
	{
		private readonly SmsClient _smsClient;
		private readonly SmsServiceType _serviceType;
		private readonly CountryCode _countryCode;

		public SmsService(SmsServiceType service, CountryCode countryCode)
		{
			_smsClient = new SmsClient();
			_serviceType = service;
			_countryCode = countryCode;
		}

		public async Task<BalanceResponse> GetAccountBalance() => await _smsClient.GetBalance();
		public async Task<Status> GetVerificationCode(int tZid, int positionOfMessage)
		{
			//start a timer to wait for 5 minutes -> if fails then notify user
			var timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
			var hasTimeElapsed = false;
			timer.Start();
			timer.Elapsed += (o, e) =>
			{
				hasTimeElapsed = true;
			};

			while (!hasTimeElapsed)
			{
				var response = await _smsClient.GetNumberStatus(tZid);
				if (response == null) return null;
				var correctService = response.FirstOrDefault(_ => _.Service.Equals(_serviceType.ToString()));

				if(correctService == null)
					goto WaitThenTryAgain;

				try
				{
					var message = correctService.Message[positionOfMessage].Message;
					return new Status
					{
						Country = correctService.Country,
						Form = correctService.Form,
						Message = message,
						Number = correctService.Number,
						Response = correctService.Response,
						Service = correctService.Service,
						Sum = correctService.Sum,
						Time = correctService.Time,
						Tzid = correctService.Tzid
					};
				}
				catch
				{
					// wait then try later
				}

				WaitThenTryAgain:
				await Task.Delay(TimeSpan.FromSeconds(5));
			}

			return null;
		}
		public async Task<Status> IssueNumber(bool forceNew = false)
		{
			if(forceNew)
				goto IssueANewNumber;

			var operationsOnService = await _smsClient.GetCurrentOperations(_serviceType);
			if (!operationsOnService.Any())
				goto IssueANewNumber;

			var existingNumberStatus = await _smsClient.GetNumberStatus(operationsOnService.First().Tzid);
			var existResponse = existingNumberStatus.First(_=>_.Service.Equals(_serviceType.ToString()));

			if (existResponse == null)
				return null;
			
			return new Status
			{
				MessageCount = existResponse.Message?.Length ?? 0,
				IsNewNumber = false,
				Country = existResponse.Country,
				Form = existResponse.Form,
				Message = null,
				Number = existResponse.Number,
				Response = existResponse.Response,
				Service = existResponse.Service,
				Sum = existResponse.Sum,
				Tzid = existResponse.Tzid,
				Time = existResponse.Time
			};

			IssueANewNumber:
			var newNumber = await _smsClient.IssueNumberForService(_countryCode, _serviceType);
			if (newNumber == null || newNumber.Tzid <= 0)
				return null;
			var numberStatus = await _smsClient.GetNumberStatus(newNumber.Tzid);

			var numberResponse = numberStatus?.First(_ => _.Service.Equals(_serviceType.ToString()));
			if (numberResponse == null)
				return null;

			return new Status
			{
				IsNewNumber = true,
				Number = numberResponse.Number,
				Country = numberResponse.Country,
				Form = numberResponse.Form,
				Message = null,
				Response = numberResponse.Response,
				Service = numberResponse.Service,
				Sum = numberResponse.Sum,
				Time = numberResponse.Time,
				Tzid = numberResponse.Tzid
			};
		}
	}
}
