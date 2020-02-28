using OpenQA.Selenium.Chrome;
using Quarkless.Models.SeleniumClient.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using Microsoft.Extensions.Options;
using Quarkless.Models.SeleniumClient;

namespace Quarkless.Logic.SeleniumClient
{
	public class SeleniumClient : ISeleniumClient
	{
		private readonly string _remoteChromeEndpoint;
		private ChromeOptions ChromeOptions { get; }

		public SeleniumClient(IOptions<SeleniumLaunchOptions> options)
		{
			_remoteChromeEndpoint = options.Value.ChromePath;
			ChromeOptions = new ChromeOptions()
			{
				LeaveBrowserRunning = false,
				AcceptInsecureCertificates = true,
				PageLoadStrategy = PageLoadStrategy.Normal,
			};
			AddArguments("no-sandbox", "--disable-dev-shm-usage", "--log-level=3");
		}

		public void AddArguments(params string[] args)
		{
			foreach (var arg in args)
			{
				ChromeOptions.AddArgument(arg);
			}
		}
		public void SetProxy(Proxy proxy)
		{
			ChromeOptions.Proxy = proxy;
		}
		public IWebDriver CreateDriver()
		{

			return new RemoteWebDriver(new Uri(_remoteChromeEndpoint), ChromeOptions);
		}

		public IEnumerable<Cookie> GetCookiesOfPage(string url)
		{
			try
			{
				//using(var driver = new ChromeDriver(_chromeService, ChromeOptions))
				using var driver = CreateDriver();
				driver.Navigate().GoToUrl(url);
				return driver.Manage().Cookies.AllCookies;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public void ScrollPage(IWebDriver driver, int counter)
		{
			const string script =
				@"window.scrollTo(0,Math.max(document.documentElement.scrollHeight,document.body.scrollHeight,document.documentElement.clientHeight));";

			var count = 0;

			while (count != counter)
			{
				var js = driver as IJavaScriptExecutor;
				js.ExecuteScript(script);

				Thread.Sleep(500);

				count++;
			}
		}

		public void ScrollPageByPixel(IWebDriver driver, int yAmount)
		{
			var current = 0;
			var amount = 2500;
			while (current < yAmount)
			{
				var script = $@"window.scrollBy(0, {amount})";

				var jsExecute = driver as IJavaScriptExecutor;

				jsExecute.ExecuteScript(script);

				current ++;
				Thread.Sleep(5000);
			}
		}

		public void ScrollToElement(IWebDriver driver, int positionX, int positionY)
		{
			var script = $@"window.scrollTo({positionX},{positionY});";
			var js = driver as IJavaScriptExecutor;
			js.ExecuteScript(script);
			Thread.Sleep(500);
		}
	}
}
