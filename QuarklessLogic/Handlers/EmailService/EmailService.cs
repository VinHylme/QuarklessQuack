using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.FakerModels;
using QuarklessContexts.Models.Proxies;
using QuarklessLogic.ContentSearch.SeleniumClient;
using QuarklessLogic.Handlers.ReportHandler;

namespace QuarklessLogic.Handlers.EmailService
{
	public class EmailService : IEmailService
	{
		private readonly ISeleniumClient _seleniumClient;
		private readonly IReportHandler _reportHandler;
		public EmailService(ISeleniumClient seleniumClient, IReportHandler reportHandler)
		{
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("EmailService");
			_seleniumClient = seleniumClient;
			_seleniumClient.AddArguments(
				//"--headless",
				"--enable-features=NetworkService"
			);
		}

		public async Task CreateGmailEmail(ProxyModel proxy, FakerModel person)
		{
			if (proxy != null)
			{
				var proxyLine = string.IsNullOrEmpty(proxy.Username) ? $"http://{proxy.Address}:{proxy.Port}" : $"http://{proxy.Username}:{proxy.Password}@{proxy.Address}:{proxy.Port}";
				_seleniumClient.AddArguments($"--proxy-server={proxyLine}");
			}
			using (var driver = _seleniumClient.CreateDriver())
			{
				try
				{
					driver.Navigate().GoToUrl("https://accounts.google.com/signup/v2/webcreateaccount?biz=false&flowName=GlifWebSignIn&flowEntry=SignUp");
					await Task.Delay(SecureRandom.Next(1500,2000));
					driver.FindElement(By.Name("firstName")).SendKeys(person.FirstName);
					await Task.Delay(SecureRandom.Next(1500,2000));
					driver.FindElement(By.Name("lastName")).SendKeys(person.LastName);
					await Task.Delay(SecureRandom.Next(1500,2000));
					driver.FindElement(By.Name("lastName")).SendKeys(person.LastName);
					await Task.Delay(SecureRandom.Next(1500,2000));
					driver.FindElement(By.Name("Username")).SendKeys(person.Username);
					await Task.Delay(SecureRandom.Next(1500,2000));
					driver.FindElement(By.Name("Passwd")).SendKeys(person.Password);
					await Task.Delay(SecureRandom.Next(1500,2000));
					driver.FindElement(By.Name("ConfirmPasswd")).SendKeys(person.Password);
					await Task.Delay(SecureRandom.Next(1500,2000));
					driver.FindElement(By.ClassName("RveJvd")).Click();
				}
				catch (Exception ee)
				{
					Console.WriteLine(ee);
					_reportHandler.MakeReport(ee);
				}
			}
		}
	}
}
