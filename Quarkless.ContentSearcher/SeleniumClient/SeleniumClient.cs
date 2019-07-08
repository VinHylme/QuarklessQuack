using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using QuarklessContexts.Extensions;

namespace ContentSearcher.SeleniumClient
{
	public class SeleniumClient : ISeleniumClient
	{
		internal IWebDriver Driver { get; set; }
		private readonly ChromeDriverService _chromeService;
		private ChromeOptions _chromeOptions { get; set; }

		public SeleniumClient()
		{
			var path = Environment.CurrentDirectory;
			 _chromeService = ChromeDriverService.CreateDefaultService(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\chrome");
			_chromeOptions = new ChromeOptions()
			{
				LeaveBrowserRunning = false,
				AcceptInsecureCertificates = true,
				PageLoadStrategy = PageLoadStrategy.Normal
			};
		}
		public void AddProxy(Proxy proxy)
		{
			_chromeOptions.Proxy = proxy;
		}
		public void Initialise()
		{
			Driver = new ChromeDriver(_chromeService, _chromeOptions);
		}
		public void ScrollPage(int counter)
		{
			const string script =
				@"window.scrollTo(0,Math.max(document.documentElement.scrollHeight,document.body.scrollHeight,document.documentElement.clientHeight));";

			int count = 0;

			while (count != counter)
			{
				IJavaScriptExecutor js = Driver as IJavaScriptExecutor;
				js.ExecuteScript(script);

				Thread.Sleep(500);

				count++;
			}
		}
		public void ScrollToElement(int positionX, int positionY)
		{
			string script = $@"window.scrollTo({positionX},{positionY});";
			IJavaScriptExecutor js = Driver as IJavaScriptExecutor;
			js.ExecuteScript(script);
			Thread.Sleep(500);
		}
		public IEnumerable<string> DetectLangauge(string url, string targetElement, params string[] data)
		{
			try
			{
				using (Driver = new ChromeDriver(_chromeService, _chromeOptions))
				{
					List<string> results = new List<string>();
					foreach (var text in data)
					{
						var urlreq = string.Format(url, text.Replace(" ", "%20"));
						Driver.Navigate().GoToUrl(urlreq);
						IWebElement searchBox = Driver.FindElement(By.Id("source"));
						TextCopy.Clipboard.SetText(text);
						searchBox.SendKeys(OpenQA.Selenium.Keys.Control + 'v');
						Thread.Sleep(800);
						List<IWebElement> elements_results = Driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]")).ToList();
						results.AddRange(elements_results.Select(a => a.Text).Where(c => c.Contains("-")));
					}
					return results;
				}
			}
			catch (Exception ee)
			{
				return null;
			}
		}

		public IEnumerable<string> DetectLanguageViaGoogle(string url, string targetElement,
			bool getValues = false, params string[] data)
		{
			List<string> results = new List<string>();
			try
			{
				using (IWebDriver Driver_ = new ChromeDriver(ChromeDriverService.CreateDefaultService(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\chrome\"), _chromeOptions))
				{
					foreach (var text in data)
					{
						var urlreq = string.Format(url, text);
						Driver_.Navigate().GoToUrl(urlreq);
						Thread.Sleep(22);
						List<IWebElement> elements = Driver_.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]")).ToList();

						if (elements == null) { throw new Exception("results empty"); }
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

		public IEnumerable<string> YandexImageSearch(string url, string imageurl, string targetElement, int limit = 5,  string patternRegex = @"(http(s?):)([/|.|\w|\s|-])*\.(?:jpg|gif|png)")
		{
			try
			{
				using(Driver = new ChromeDriver(_chromeService, _chromeOptions))
				{
					Driver.Navigate().GoToUrl(url);

					IWebElement searchButton = Driver.FindElement(By.ClassName("input__button"));
					TextCopy.Clipboard.SetText(imageurl);
					
					searchButton.Click();
					IWebElement searchField = Driver.FindElement(By.Name("cbir-url"));
					searchField.SendKeys(Keys.Control + 'v');
					IWebElement submitButton = Driver.FindElement(By.Name("cbir-submit"));
					submitButton.Click();
					Thread.Sleep(2000);

					IWebElement similarButton = Driver.FindElement(By.ClassName("similar__link"));
					//var hrefval = similarButton.GetAttribute("href");

					((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", similarButton);
					Thread.Sleep(3000);
					Driver.FindElement(By.ClassName("pane2__close-icon")).Click();
					List<IWebElement> elements = Driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]")).ToList();

					if (elements == null) { throw new Exception("results empty"); }

					while (elements.Count < limit)
					{
						ScrollPage(1);
						var uniqueFind = Driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]"));
						elements.AddRange(uniqueFind);
						elements = elements.Distinct().ToList();
					}

					List<string> urls = new List<string>();
					for (int x = 0; x <= limit; x++)
					{
						var tohtml = elements[x].GetAttribute("outerHTML");

						var decoded = HttpUtility.HtmlDecode(tohtml);
						var imageUrl = Regex.Matches(decoded, patternRegex).Select(_ => _.Value).FirstOrDefault();
						if (string.IsNullOrEmpty(imageUrl))
						{
							if (x > elements.Count)
							{
								throw new Exception("can't find any images, all returning null");
							}
							x = x == 0 ? 0 : x--;
							continue;
						}
						else
						{
							urls.Add(imageUrl);
						}
					}
					return urls.TakeAny(SecureRandom.Next(urls.Count));
				}
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public IEnumerable<string> Reader(string url, string targetElement, int limit = 5, string patternRegex = @"(http(s?):)([/|.|\w|\s|-])*\.(?:jpg|gif|png)")
		{
			try
			{
				using (Driver = new ChromeDriver(_chromeService, _chromeOptions))
				{
					Driver.Navigate().GoToUrl(url);
					List<IWebElement> elements = Driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]")).ToList();

					if (elements == null && elements.Count<=0) { return null; }
					while (elements.Count < limit)
					{
						ScrollPage(1);
						var uniqueFind = Driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]"));
						elements.AddRange(uniqueFind);
						elements = elements.Distinct().ToList();
					}

					List<string> urls = new List<string>();
					for (int x = 0; x < limit; x++)
					{
						var tohtml = elements[x].GetAttribute("outerHTML");

						var decoded = HttpUtility.HtmlDecode(tohtml);
						var imageUrl = Regex.Matches(decoded, patternRegex).Select(_ => _.Value).FirstOrDefault();
						if (string.IsNullOrEmpty(imageUrl))
						{
							if (x > elements.Count)
							{
								throw new Exception("can't find any images, all returning null");
							}
							x = x == 0 ? 0 : x--;
							continue;
						}
						else
						{
							urls.Add(imageUrl);
						}
					}
					var waitResults = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
					return urls;
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee);
				return null;
			}
		}
		public void AddArguments(params string[] args)
		{
			foreach (var arg in args)
			{
				_chromeOptions.AddArgument(arg);
			}
		}
	}
}
