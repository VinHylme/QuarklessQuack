using Microsoft.Extensions.Options;
using Quarkless.Models.Proxy;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using Quarkless.Models.TranslateService;
using Quarkless.Models.TranslateService.Interfaces;
using System;
using System.Collections.Generic;

namespace Quarkless.Logic.TranslateService
{
	//TODO: Class NEEDS TO BE REFACTORED
	public class TranslateService : ITranslateService
	{
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly string _yandexApiKey;
		private readonly string _detectApiKey;
		public TranslateService(IOptions<TranslateOptions> tOptions,
			IRestSharpClientManager restSharpClient)
		{
			_restSharpClient = restSharpClient;
			_yandexApiKey = tOptions.Value.YandexAPIKey;
			_detectApiKey = tOptions.Value.DetectLangAPIKey;
		}

		public void AddProxy(ProxyModel proxy)
		{
			if (proxy == null) return;
			_restSharpClient.AddProxy(proxy);
		}

		public IEnumerable<string> DetectLanguage(params string[] texts)
		{
			throw new NotImplementedException();
		}
	}
}
