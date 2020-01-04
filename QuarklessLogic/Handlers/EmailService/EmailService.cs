using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using OpenQA.Selenium;
using QuarklessLogic.ContentSearch.SeleniumClient;

namespace QuarklessLogic.Handlers.EmailService
{
	public class EmailService : IEmailService
	{
		private const string GMAIL_URL_LOGIN = "https://accounts.google.com/signin/v2/identifier?continue=https%3A%2F%2Fmail.google.com%2Fmail%2F&service=mail&sacu=1&rip=1&flowName=GlifWebSignIn&flowEntry=ServiceLogin";
		private const string GMAIL_MAIL_SERVER = "imap.gmail.com";
		private const int GMAIL_MAIL_PORT = 993;
		private readonly string _mailServer;
		private readonly int _port;
		private readonly bool _ssl;
		private readonly ISeleniumClient _seleniumClient;
		public EmailService(ISeleniumClient seleniumClient)
		{
			_seleniumClient = seleniumClient;
			_seleniumClient.AddArguments(
			//	"headless",
				"--ignore-certificate-errors");
			_mailServer = GMAIL_MAIL_SERVER;
			_port = GMAIL_MAIL_PORT;
			_ssl = true;
		}

		public async Task<IWebDriver> LoginToGmailAccount(string email, string password)
		{
			try
			{
				using (var client = _seleniumClient.CreateDriver())
				{
					client.Navigate().GoToUrl(new Uri(GMAIL_URL_LOGIN));
					await Task.Delay(TimeSpan.FromSeconds(3));
					var idElement = client.FindElement(By.XPath("//input[contains(@id,'identifierId')]"));
					idElement.SendKeys(email);
					var nextButton = client.FindElement(By.XPath("//div[contains(@id,'identifierNext')]"));
					nextButton.Click();
					await Task.Delay(TimeSpan.FromSeconds(2));
					var passElement = client.FindElement(By.Name("password"));
					passElement.SendKeys(password);
					var passNextButton = client.FindElement(By.Id("passwordNext"));
					passNextButton.Click();
					await Task.Delay(TimeSpan.FromSeconds(4));
					var p = client.PageSource;
					var dashboard = client.FindElement(By.Id(":l2"));
					if (dashboard != null)
						return client;
				}
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}

			return null;
		}

		public async Task<List<string>> GetUnreadEmails(string email, string password)
		{
			var messages = new List<string>();
			using (var client = await LoginToGmailAccount(email, password))
			{
				if (client == null)
					return messages;
				var tr = client.FindElements(By.XPath("//div[@class='zA zE']"));

			}

			return messages;
		}
		public IEnumerable<string> GetUnreadMails(string login, string password)
		{
			var messages = new List<string>();
			try
			{
				using (var client = new ImapClient())
				{
					client.Connect(_mailServer, _port, _ssl);

					client.AuthenticationMechanisms.Remove("XOAUTH2");

					client.Authenticate(login, password);

					var inbox = client.Inbox;
					inbox.Open(FolderAccess.ReadOnly);
					var results = inbox.Search(SearchOptions.All, SearchQuery.Not(SearchQuery.Seen));
					foreach (var uniqueId in results.UniqueIds)
					{
						var message = inbox.GetMessage(uniqueId);

						messages.Add(message.HtmlBody);
					}

					client.Disconnect(true);
				}
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}

			return messages;
		}
    }
}
