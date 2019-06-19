using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContentSearcher.SeleniumClient
{
	public interface ISeleniumClient
	{
		void AddArguments(params string[] args);
		void Initialise();
		IEnumerable<string> DetectLangauge(string url, string targetElement, params string[] data);
		IEnumerable<string> DetectLanguageViaGoogle(string url, string targetElement, bool getValues = false, params string[] data);
		IEnumerable<string> Reader(string url, string targetElement, int limit = 5, string patternRegex = "(http(s?):)([/|.|\\w|\\s|-])*\\.(?:jpg|gif|png)");
	}
}