using ContentSearcher.SeleniumClient;
using Nancy.Json;
using Newtonsoft.Json;
using QuarklessContexts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuarklessLogic.Handlers.TranslateService
{
	public class TranslateService : ITranslateService
	{
		private readonly ISeleniumClient _seleniumClient;
		public TranslateService(ISeleniumClient seleniumClient)
		{
			_seleniumClient = seleniumClient;
			_seleniumClient.AddArguments(
				//"--headless",
				"--no-sandbox",
				"--disable-web-security",
				"--allow-running-insecure-content",
				"--enable-features=NetworkService"
			);
			//_seleniumClient.Initialise();
		}

		public IEnumerable<string> DetectLanguageViaGoogle(bool selectMostOccuring = false, string splitPattern = "-", params string[] @texts)
		{
			var url = "https://translate.google.com/#view=home&op=translate&sl=auto&tl=en&text={0}";
			string pattern = "goog-inline-block jfk-button jfk-button-standard jfk-button-collapse-right jfk-button-checked";
			var items = texts.Where(_ => !string.IsNullOrEmpty(_)).ToArray().CleanText().ToArray();
			var res = _seleniumClient.DetectLanguageViaGoogle(url, pattern, true, items);
			if(res==null) return null;
			if (!selectMostOccuring) 
				return splitPattern!=null ? res.Select(rf=>rf.Split(splitPattern)[0]) : res;
			else
			{
				return res.GroupBy(x=>x).OrderByDescending(g=>g.Count()).Select(g=>g.Key);
			}
		}
	}
}
