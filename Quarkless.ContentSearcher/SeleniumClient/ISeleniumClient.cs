using OpenQA.Selenium;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContentSearcher.SeleniumClient
{
	public interface ISeleniumClient
	{
		IEnumerable<string> YandexImageSearch(string url, string imageurl,string targetElement, int limit = 5, string patternRegex = @"(http(s?):)([/|.|\w|\s|-])*\.(?:jpg|gif|png)");
		void AddArguments(params string[] args);
		void Initialise();
		void AddProxy(Proxy proxy);
		IEnumerable<string> DetectLangauge(string url, string targetElement, params string[] data);
		IEnumerable<string> DetectLanguageViaGoogle(string url, string targetElement, bool getValues = false, params string[] data);
		IEnumerable<string> Reader(string url, string targetElement, int limit = 5, string patternRegex = "(http(s?):)([/|.|\\w|\\s|-])*\\.(?:jpg|gif|png)");
	}
}