using OpenQA.Selenium;
using System.Collections.Generic;

namespace Quarkless.Models.SeleniumClient.Interfaces
{
	public interface ISeleniumClient
	{
		void SetProxy(Proxy proxy);
		void AddArguments(params string[] args);
		IEnumerable<Cookie> GetCookiesOfPage(string url);
		IWebDriver CreateDriver();
		void ScrollPage(IWebDriver driver, int counter);
		void ScrollToElement(IWebDriver driver, int positionX, int positionY);
	}
}
