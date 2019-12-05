using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Options;
using QuarklessContexts.Models.ResponseModels;
using QuarklessLogic.ContentSearch.YandexSearch;
using Cookie = OpenQA.Selenium.Cookie;

namespace QuarklessLogic.ContentSearch.SeleniumClient
{

	public class SeleniumClient : ISeleniumClient
	{
		private readonly object _locker = new object();
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
		public IEnumerable<string> DetectLanguage(string url, string targetElement, params string[] data)
		{
			try
			{
				using (var driver = CreateDriver())
				{
					var results = new List<string>();
					foreach (var text in data)
					{
						var urlreq = string.Format(url, text.Replace(" ", "%20"));
						driver.Navigate().GoToUrl(urlreq);
						var searchBox = driver.FindElement(By.Id("source"));
						TextCopy.Clipboard.SetText(text);
						searchBox.SendKeys(OpenQA.Selenium.Keys.Control + 'v');
						Thread.Sleep(800);
						List<IWebElement> elementsResults = driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]")).ToList();
						results.AddRange(elementsResults.Select(a => a.Text).Where(c => c.Contains("-")));
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
			var results = new List<string>();
			try
			{
				using (var driver = CreateDriver())
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
		public SearchResponse<List<SerpItem>> YandexSearchMe(string url, int pages, int offset = 0)
		{
			try
			{
				var totalCollected = new List<SerpItem>();
				var response = new SearchResponse<List<SerpItem>>();
				using (var driver = CreateDriver())
				{
					driver.Navigate().GoToUrl(url);
					if (driver.Url.Contains("captcha"))
					{
						response.StatusCode = ResponseCode.CaptchaRequired;
						response.Message = "Yandex captcha needed";
						return response;
					}

					totalCollected.AddRange(ReadSerpItems(driver, pages, offset));
				}

				response.StatusCode = ResponseCode.Success;
				response.Result = totalCollected;
				return response;
			}
			catch (Exception io)
			{
				Console.WriteLine(io.Message);
				return null;
			}
		}
		public IEnumerable<string> YandexImageSearch(string url, string imageurl, string targetElement, int limit = 5,  string patternRegex = @"(http(s?):)([/|.|\w|\s|-])*\.(?:jpg|gif|png)")
		{
			try
			{
				var urls = new List<string>();
				using (var driver = CreateDriver())
				{
					driver.Navigate().GoToUrl(url);

					var searchButton = driver.FindElement(By.ClassName("input__button"));
					TextCopy.Clipboard.SetText(imageurl);
					
					searchButton.Click();
					var searchField = driver.FindElement(By.Name("cbir-url"));
					searchField.SendKeys(Keys.Control + 'v');
					var submitButton = driver.FindElement(By.Name("cbir-submit"));
					submitButton.Click();
					
					var pagehere = driver.PageSource;
					Thread.Sleep(2000);

					var similarButton = driver.FindElement(By.ClassName("similar__link"));
					//var hrefval = similarButton.GetAttribute("href");

					((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", similarButton);
					Thread.Sleep(3000);
					driver.FindElement(By.ClassName("pane2__close-icon")).Click();
					var elements = driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]")).ToList();

					while (elements.Count < limit)
					{
						ScrollPage(driver,1);
						var uniqueFind = driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]"));
						elements.AddRange(uniqueFind);
						elements = elements.Distinct().ToList();
					}

					for (var x = 0; x <= limit; x++)
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
		private IEnumerable<SerpItem> ReadSerpItems(IWebDriver driver, int pageLimit, int offset = 0)
		{
			var total = new List<SerpItem>();
			for (var currPage = offset; currPage <= pageLimit; currPage++)
			{
				var source = driver.PageSource.Replace("&quot;", "\"");
				var regexMatch = Regex.Matches(source, "{\"serp-item\":.*?}}");		
				var results = new List<string>();
				foreach(Match x in regexMatch)
				{
					var newRes = x.Value.Replace("{\"serp-item\":", "");
					results.Add(newRes.Substring(0, newRes.Length - 1));
				}
				if (results.Count <= 0)
				{
					break;
				}

				foreach(var serpString in results)
				{
					try
					{
						total.Add(JsonConvert.DeserializeObject<SerpItem>(serpString));
					}
					catch
					{
						var tryagainSerpString = serpString + "}}";
						total.Add(JsonConvert.DeserializeObject<SerpItem>(tryagainSerpString));
						//Console.WriteLine("could not convert serp object");
					}
				}
				if(total.Count>0)
					total.RemoveAt(0); //remove the duplicate

				var nextPageUrl = driver.FindElement(By.ClassName("more__button")).GetAttribute("href");
				Thread.Sleep(1000);
				driver.Navigate().GoToUrl(nextPageUrl);
			}
			return total;
		}
		public SearchResponse<List<SerpItem>> Reader(string url, int limit = 1)
		{
			try
			{
				lock (_locker)
				{
					var totalCollected = new List<SerpItem>();
					var response = new SearchResponse<List<SerpItem>>();
					using (var driver = CreateDriver())
					{
						driver.Navigate().GoToUrl(url);
						if (driver.Url.Contains("captcha"))
						{
							response.StatusCode = ResponseCode.CaptchaRequired;
							response.Message = "Yandex captcha needed";
							return response;
						}

						Thread.Sleep(500);
						try
						{
							var misspell = driver.FindElement(By.ClassName("misspell__message"));
							if (misspell != null)
							{
								var autosuggestion = "https://yandex.com" +
								                     Regex.Match(misspell.GetAttribute("outerHTML"),
										                     "href=.*?/images.*?>").Value.Replace("href=", "")
									                     .Replace(">", "").Replace("\"", "");
								driver.Navigate().GoToUrl(autosuggestion);
								Thread.Sleep(2000);
							}
						}
						catch
						{
							Console.WriteLine("nothing suggested for yandex search query");
						}

						totalCollected.AddRange(ReadSerpItems(driver, limit));

						#region old stuff

						/*
						List<IWebElement> elements = Driver.FindElements(By.XPath($"//div[contains(@class,'{targetElement}')]")).ToList();
						if (elements == null || elements.Count<=0) { return null; }
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
						return urls;*/

						#endregion
					}

					response.StatusCode = ResponseCode.Success;
					response.Result = totalCollected;
					return response;
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee);
				return null;
			}
		}
		public IEnumerable<string> YandexImageSearchREST(string baseurl, string url, int pageLimit = 5)
		{
			try
			{
				using (var driver = CreateDriver())
				{
					driver.Navigate().GoToUrl(baseurl);
					var souce = driver.PageSource;
					driver.Navigate().GoToUrl(url);
					var manageD = driver.Manage().Cookies.AllCookies;
					var source = driver.PageSource;
				}


				return null;
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public IEnumerable<Cookie> GetCookiesOfPage(string url)
		{
			try
			{
//				using(var driver = new ChromeDriver(_chromeService, ChromeOptions))
				using(var driver = CreateDriver())
				{
					driver.Navigate().GoToUrl(url);
					return driver.Manage().Cookies.AllCookies;
				}
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}

		#region JS Functions
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
		public void ScrollToElement(IWebDriver driver, int positionX, int positionY)
		{
			var script = $@"window.scrollTo({positionX},{positionY});";
			var js = driver as IJavaScriptExecutor;
			js.ExecuteScript(script);
			Thread.Sleep(500);
		}
		#endregion
	}
}
