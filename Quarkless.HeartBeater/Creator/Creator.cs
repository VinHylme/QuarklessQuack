using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuarklessContexts.Models.Proxies;
using QuarklessLogic.ContentSearch.SeleniumClient;
using Bogus;
using Bogus.DataSets;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using QuarklessContexts.Extensions;
using QuarklessLogic.Handlers.Util;

namespace Quarkless.HeartBeater.Creator
{
	public static class WebDriverExtensions
	{
		public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
		{
			if (timeoutInSeconds <= 0) return driver.FindElement(@by);
			var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
			return wait.Until(drv => drv.FindElement(@by));
		}
	}
	public interface ICreator
	{
		Task<Creator.Tempo> CreateInstagramAccount(ProxyModel proxy = null);
	}
	public class Creator : ICreator
	{
		private readonly ISeleniumClient _seleniumClient;
		private readonly IUtilProviders _utilProviders;
		public Creator(ISeleniumClient seleniumClient, IUtilProviders utilProviders)
		{
			_seleniumClient = seleniumClient;
			_utilProviders = utilProviders;
			_seleniumClient.AddArguments(
				//"headless",
				"--log-level=3",
				"--silent",
				"--disable-extensions",
				"test-type",
				"--ignore-certificate-errors",
				"no-sandbox");
		}
		public class Tempo
		{
			public Name.Gender Gender;
			public string FirstName;
			public string LastName;
			public string Username;
			public string Password;
			public string UserAgent;
			public string Email;
		}
		public async Task<Tempo> CreateInstagramAccount(ProxyModel proxy = null)
		{
			Tempo tempo = null;
			if (proxy != null)
			{
				var proxyLine = string.IsNullOrEmpty(proxy.Username) ? $"http://{proxy.Address}:{proxy.Port}" : $"http://{proxy.Username}:{proxy.Password}@{proxy.Address}:{proxy.Port}";
				_seleniumClient.AddArguments($"--proxy-server={proxyLine}");
			}

			var person = _utilProviders.GeneratePerson(emailProvider:"gmai.com");
			_seleniumClient.AddArguments($"user-agent={person.UserAgent}");
			using (var driver = _seleniumClient.Driver)
			{
				driver.Navigate().GoToUrl("https://www.instagram.com/");
				driver.FindElement(By.Name("emailOrPhone"),5).SendKeys(person.Email);
				await Task.Delay(SecureRandom.Next(100,400));
				driver.FindElement(By.Name("fullName"),5).SendKeys(person.FirstName + " " + person.LastName);
				await Task.Delay(SecureRandom.Next(100,400));
				driver.FindElement(By.Name("username"),5).SendKeys(person.Username);
				await Task.Delay(SecureRandom.Next(556,1000));
				driver.FindElement(By.Name("password"),5).SendKeys(person.Password);
				await Task.Delay(SecureRandom.Next(767,1500));
				driver.FindElement(By.Name("password"),5).SendKeys(Keys.Enter);
				await Task.Delay(SecureRandom.Next(1400,3000));
				driver.FindElement(By.Id("igCoreRadioButtonageRadioabove_18"),5).Click();
				await Task.Delay(SecureRandom.Next(1400, 2000));
				var nextButton = driver.FindElements(By.XPath("//button[text()='Next']"));
				new Actions(driver).MoveToElement(nextButton.Last()).Click().Perform();
				Thread.Sleep(7000);

				if (!IsElementPresent(driver,By.Id("ssfErrorAlert")))
				{
					tempo = new Tempo
					{
						UserAgent = person.UserAgent,
						Email = person.Email,
						FirstName = person.FirstName,
						LastName = person.LastName,
						Gender = person.Gender,
						Password = person.Password,
						Username = person.Username
					};
				}
			}
			return tempo;
		}
		private bool IsElementPresent(IWebDriver driver, By by)
		{
			try
			{
				driver.FindElement(by);
				return true;
			}
			catch (NoSuchElementException)
			{
				return false;
			}
		}
	}
}
