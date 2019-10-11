using OpenQA.Selenium;
using QuarklessContexts.Models.ResponseModels;
using System.Collections.Generic;

namespace QuarklessLogic.ContentSearch.SeleniumClient
{
	public interface ISeleniumClient
	{
		void SetProxy(Proxy proxy);
		void TestRunFireFox();
		IEnumerable<string> YandexImageSearch(string url, string imageurl,string targetElement, int limit = 5, string patternRegex = @"(http(s?):)([/|.|\w|\s|-])*\.(?:jpg|gif|png)");
		void AddArguments(params string[] args);
		void Initialise();
		IEnumerable<string> YandexImageSearchREST(string baseurl, string url, int pageLimit = 5);
		IEnumerable<Cookie> GetCookiesOfPage(string url);
		IEnumerable<string> DetectLangauge(string url, string targetElement, params string[] data);
		IEnumerable<string> DetectLanguageViaGoogle(string url, string targetElement, bool getValues = false, params string[] data);
		SearchResponse<List<SerpItem>> Reader(string url, int limit = 5 );
		SearchResponse<List<SerpItem>> YandexSearchMe(string url, int pages, int offset = 0);
		IWebDriver Driver { get; }
	}
}