using Microsoft.Extensions.Options;
using Quarkless.Models.Proxy;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using Quarkless.Models.SeleniumClient.Interfaces;
using Quarkless.Models.TranslateService;
using Quarkless.Models.TranslateService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Newtonsoft.Json;
using OpenQA.Selenium;
using Quarkless.Models.Common.Extensions;

namespace Quarkless.Logic.TranslateService
{
	//TODO: Class NEEDS TO BE REFACTORED
	public class TranslateService : ITranslateService
	{
		private readonly ISeleniumClient _seleniumClient;
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly string _yandexApiKey;
		private readonly string _detectApiKey;
		//private readonly string _naturalLanguageProcessingApi;
		static int pos = 0;
		public TranslateService(IOptions<TranslateOptions> tOptions, ISeleniumClient seleniumClient, 
			IRestSharpClientManager restSharpClient)
		{
			_restSharpClient = restSharpClient;
			_seleniumClient = seleniumClient;
			_yandexApiKey = tOptions.Value.YandexAPIKey;
			_detectApiKey = tOptions.Value.DetectLangAPIKey;
			//_naturalLanguageProcessingApi = tOptions.Value.NaturalLanguageAPIPath;
			_seleniumClient.AddArguments(
				"--headless",
				"--enable-features=NetworkService"
			);
		}

		public void AddProxy(ProxyModel proxy)
		{
			if (proxy == null) return;
			_restSharpClient.AddProxy(proxy);
			var proxyLine = string.IsNullOrEmpty(proxy.Username) 
				? $"{proxy.HostAddress}:{proxy.Port}" 
				: $"{proxy.Username}:{proxy.Password}@{proxy.HostAddress}:{proxy.Port}";
			_seleniumClient.AddArguments($"--proxy-server={proxyLine}");
		}

		public IEnumerable<string> DetectLanguage(params string[] texts)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> DetectLanguageYandex(params string[] texts)
		{
			var keyYandex = _yandexApiKey.Split('|');
			if (pos >= keyYandex.Length)
			{
				var res = DetectLanguageRest(texts);
				if (res == null) yield break;
				foreach (var det in res)
				{
					if (det != null)
					{
						yield return det;
					}
				}
			}
			else 
			{ 
				var url = "https://translate.yandex.net/api/v1.5/tr.json/detect?key="+keyYandex[pos]+"&text=";
				foreach(var text in texts)
				{
					var postRes = _restSharpClient.PostRequest(url + HttpUtility.UrlEncode(text), null, null);
					if (postRes == null || string.IsNullOrEmpty(postRes.Content)) continue;
					var yanRes = JsonConvert.DeserializeObject<YandexLanguageResults>(postRes.Content);
					if (yanRes.Code != 404 && yanRes.Code!=401)
					{
						yield return yanRes.Lang;
					}
					else
					{
						pos++;
						DetectLanguageYandex(texts);
					}
				}
			}
		}
		public IEnumerable<string> DetectLanguageRest(params string[] texts)
		{

			var url = "https://ws.detectlanguage.com/";
			var res = _restSharpClient.PostRequest(url,"/0.2/detect",JsonConvert.SerializeObject(new { q = texts }),username: _detectApiKey ,password:"");
			if (!res.IsSuccessful) return DetectLanguageViaGoogle(texts: texts);
			var batchResult = JsonConvert.DeserializeObject<BatchResult>(res.Content);
			return batchResult.data.detections.Select(s => s.FirstOrDefault()?.language);

		}
		public IEnumerable<string> DetectLanguageViaGoogle(bool selectMostOccuring = false, string splitPattern = "-", params string[] @texts)
		{
			var url = "https://translate.google.com/#view=home&op=translate&sl=auto&tl=en&text={0}";
			var pattern = "goog-inline-block jfk-button jfk-button-standard jfk-button-collapse-right jfk-button-checked";
			var items = texts.Where(_ => !string.IsNullOrEmpty(_)).ToArray().CleanText().ToArray();
			var res = DetectLanguageViaGoogle(url, pattern, true, items);
			if(res==null) return null;
			if (!selectMostOccuring) 
				return splitPattern!=null ? res.Select(rf=>rf.Split(splitPattern)[0]) : res;
			else
			{
				return res.GroupBy(x=>x).OrderByDescending(g=>g.Count()).Select(g=>g.Key);
			}
		}
		public IEnumerable<string> DetectLanguage(string url, string targetElement, params string[] data)
		{
			try
			{
				using (var driver = _seleniumClient.CreateDriver())
				{
					var results = new List<string>();
					foreach (var text in data)
					{
						var urlRequestFormat = string.Format(url, text.Replace(" ", "%20"));
						driver.Navigate().GoToUrl(urlRequestFormat);
						var searchBox = driver.FindElement(By.Id("source"));
						//TextCopy.Clipboard.SetText(text);
						searchBox.SendKeys(text);
						Thread.Sleep(800);
						var elementsResults = driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]")).ToList();
						results.AddRange(elementsResults.Select(a => a.Text).Where(c => c.Contains("-")));
					}
					return results;
				}
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}
		public IEnumerable<string> DetectLanguageViaGoogle(string url, string targetElement,
			bool getValues = false, params string[] data)
		{
			var results = new List<string>();
			try
			{
				using (var driver = _seleniumClient.CreateDriver())
				{
					foreach (var text in data)
					{
						var urlreq = string.Format(url, text);
						driver.Navigate().GoToUrl(urlreq);
						Thread.Sleep(22);
						var elements = driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]")).ToList();

						if (!getValues)
						{
							results.Add(elements.Select(a => a.GetAttribute("outerHTML")).SingleOrDefault());
						}
						else
						{
							results.AddRange(elements.Select(a => a.Text).Where(c => c.Contains("-")));
						}
					}
				}
				return results;
			}
			catch (Exception ee)
			{
				Console.Write(ee.Message);
				return null;
			}
		}
	}
}
