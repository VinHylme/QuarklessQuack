using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ResponseModels;

namespace QuarklessLogic.ContentSearch.SeleniumClient
{

	class FirefoxOptionsEx : FirefoxOptions {

		public new string Profile { get; set; }

		public override ICapabilities ToCapabilities() {

			var capabilities = (DesiredCapabilities)base.ToCapabilities();
			var options = (IDictionary)capabilities.GetCapability("moz:firefoxOptions");
			var mstream = new MemoryStream();

			using (var archive = new ZipArchive(mstream, ZipArchiveMode.Create, true)) {
				foreach (string file in Directory.EnumerateFiles(Profile, "*", SearchOption.AllDirectories)) {
					string name = file.Substring(Profile.Length + 1).Replace('\\', '/');
					if (name != "parent.lock") {
						using (Stream src = File.OpenRead(file), dest = archive.CreateEntry(name).Open())
							src.CopyTo(dest);
					}
				}
			}

			options["profile"] = Convert.ToBase64String(mstream.GetBuffer(), 0, (int)mstream.Length);

			return capabilities;
		}

	}
	public class SeleniumClient : ISeleniumClient
	{
		//internal IWebDriver Driver { get; set; }
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

		public void TestRunFireFox()
		{
			var profile = new FirefoxProfile
			{
				DeleteAfterUse = true
			};

			profile.AddExtension(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\firefox\extensions\canvasProtec.xpi");
			var service = FirefoxDriverService.CreateDefaultService(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\firefox");
			
			var options = new FirefoxOptionsEx()
			{
				PageLoadStrategy = PageLoadStrategy.Normal,
			};

			var manager = new FirefoxProfileManager();
			var profiles = (Dictionary<string, string>)manager.GetType()
				.GetField("profiles", BindingFlags.Instance | BindingFlags.NonPublic)
				.GetValue(manager);

			string directory;
			if (profiles.TryGetValue("default", out directory))
				options.Profile = directory;

			//options.Profile = profile;
			options.SetPreference("network.proxy.type", 0);

			using (var driver = new FirefoxDriver(service, options))
			{
				driver.Navigate().GoToUrl("https://www.google.com");
			}
		}

		public void Initialise()
		{
			//Driver = new ChromeDriver(_chromeService, _chromeOptions);
		}

		public IWebDriver Driver => new ChromeDriver(_chromeService, _chromeOptions);
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
		public IEnumerable<string> DetectLangauge(string url, string targetElement, params string[] data)
		{
			try
			{
				using (var driver = new ChromeDriver(_chromeService, _chromeOptions))
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
				using (IWebDriver driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\chrome\"), _chromeOptions))
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
				using (var driver = new ChromeDriver(_chromeService, _chromeOptions))
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
				List<string> urls = new List<string>();
				using(var driver = new ChromeDriver(_chromeService, _chromeOptions))
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
		private List<SerpItem> ReadSerpItems(IWebDriver driver, int pageLimit, int offset = 0)
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

		private object _locker = new object();
		public SearchResponse<List<SerpItem>> Reader(string url, int limit = 1)
		{
			try
			{
				lock (_locker)
				{
					var totalCollected = new List<SerpItem>();
					var response = new SearchResponse<List<SerpItem>>();
					using (var driver = new ChromeDriver(_chromeService, _chromeOptions))
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
				using(var driver = new ChromeDriver(_chromeService, _chromeOptions))
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
				using(var driver = new ChromeDriver(_chromeService, _chromeOptions))
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
		public void AddArguments(params string[] args)
		{
			foreach (var arg in args)
			{
				_chromeOptions.AddArgument(arg);
			}
		}
	}
}
