using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using PuppeteerSharp;
using Quarkless.EmailServices.Models;
using Quarkless.EmailServices.Models.Enums;
using Quarkless.EmailServices.Repository;
using Quarkless.PuppeteerClient.Logic;
using Quarkless.PuppeteerClient.Models.Enums;
using Quarkless.PuppeteerClient.Models.Extensions;
using Quarkless.SmsHandler.Logic;
using Quarkless.SmsHandler.Models.Enums;
using Quarkless.Utilities.Models.Constants;
using Quarkless.Utilities.Models.Person;

namespace Quarkless.EmailServices.Logic.GmailService
{
	public class GmailClient : IGmailClient
	{
		private const string GOOGLE_CREATE_URL = "https://accounts.google.com/signup/v2/webcreateaccount?service=mail&continue=https%3A%2F%2Fmail.google.com%2Fmail%2F%3Fpc%3Dtopnav-about-n-en&hl=en-GB&flowName=GlifWebSignIn&flowEntry=SignUp";
		private const string GMAIL_LOGIN_URL = "https://accounts.google.com/signin/v2/identifier?service=mail&passive=true&rm=false&continue=https%3A%2F%2Fmail.google.com%2Fmail%2F&ss=1&scc=1&ltmpl=default&ltmplcache=2&emr=1&osid=1&flowName=GlifWebSignIn&flowEntry=ServiceLogin";

		private readonly PuppeteerHandler _puppeteerHandler;
		private readonly Random _random;
		private readonly IEmailAccountCreatorRepository _gmailRepository;
		public GmailClient(IEmailAccountCreatorRepository gmailRepository)
		{
			_gmailRepository = gmailRepository;
			_puppeteerHandler = new PuppeteerHandler();
			_random = new Random(Environment.TickCount);
		}

		private string ScrollInsideDiv(string selector, int scrollAmount)
		{
			return @"()=>{const element = document.getElementsByClassName('"+selector+"')[0]; element.scrollTop+="+ scrollAmount +"; return true;}";
		}
		private string ClickScript(string selector)
		{
			return @"()=>{document.querySelector("+selector+").click(); return true;}";
		}

		public async Task<EmailAccount> CreateGmailAccount(PersonCreateModel person)
		{
			EmailAccount createdAccount = null;
			var proxyLine = person.Proxy != null
				? $"--proxy-server=http://{person.Proxy.IP}:{person.Proxy.Port}"
				: null;
			using (var browser = await _puppeteerHandler.OpenBrowser(proxyLine))
			{
				browser.Closed += (o, e) =>
				{
					_puppeteerHandler.RemoveBrowserContext(browser);
				};
				using (var page = await browser.NewPageAsync())
				{
					try
					{
						await page.InjectAntiScript();
						await page.SetUserAgentAsync(person.UserAgent ?? OtherConstants.DEFAULT_USER_AGENT);

						if (person.Proxy != null)
							await page.AuthenticateAsync(new Credentials
								{Password = person.Proxy.Password, Username = person.Proxy.Username});

						await page.GoToAsync(GOOGLE_CREATE_URL);
						await Task.Delay(TimeSpan.FromSeconds(5));
						#region Name Fields
						var firstName = await page.WaitForSelectorAsync("input[name=firstName]");
						await firstName.ClickAsync();
						await firstName.TypeAsync(person.FirstName, _puppeteerHandler.GetTypeOptions());
						var lastName = await page.WaitForSelectorAsync("input[name=lastName]");
						await lastName.ClickAsync();
						await lastName.TypeAsync(person.LastName, _puppeteerHandler.GetTypeOptions());
						#endregion

						#region Email Section
						var pos = 0;
						var emailField = await page.WaitForSelectorAsync("input[name=Username]");
						await emailField.ClickAsync();
						await emailField.TypeAsync(person.PossibleEmails[pos].Split("@")[0]
							.Replace("-", ".")
							.Replace("_", "."), _puppeteerHandler.GetTypeOptions(TypeSpeed.Slow));
						#endregion

						#region Password Section
						var pass1Field = await page.WaitForSelectorAsync("input[name=Passwd]");
						await pass1Field.ClickAsync();
						await pass1Field.TypeAsync(person.Password, _puppeteerHandler.GetTypeOptions(TypeSpeed.Slow));

						var pass2Field = await page.WaitForSelectorAsync("input[name=ConfirmPasswd]");
						await pass2Field.ClickAsync();
						await pass2Field.TypeAsync(person.Password,
							_puppeteerHandler.GetTypeOptions(TypeSpeed.Slowest));

						#endregion

						#region NextButton
						var nextButton = await page.WaitForSelectorAsync("#accountDetailsNext");
						await nextButton.ClickAsync(_puppeteerHandler.DoubleClick);
						await Task.Delay(TimeSpan.FromSeconds(10));
						#endregion

						#region Deal with non unique emails
						Task<ElementHandle> badEmailTask, phoneNumberBoxTask;
						for (; pos < person.PossibleEmails.Length; ++pos)
						{
							badEmailTask = page.WaitForSelectorAsync("div.GQ8Pzc");
							var tokenSource2 = new CancellationTokenSource();
							phoneNumberBoxTask = new Task<ElementHandle>(() =>
							{
								try
								{
									return page.WaitForSelectorAsync("#phoneNumberId").Result;
								}
								catch
								{
									return null;
								}
							}, tokenSource2.Token);
							phoneNumberBoxTask.Start();

							Task.WaitAny(badEmailTask, phoneNumberBoxTask);

							if (phoneNumberBoxTask.IsCompleted)
							{
								goto breakRegion;
							}

							tokenSource2.Cancel();
							pos++;
							await emailField.ClickAsync(_puppeteerHandler.DoubleClick);
							await emailField.TypeAsync(person.PossibleEmails[pos].Split("@")[0]
								.Replace("-", ".")
								.Replace("_", "."), _puppeteerHandler.GetTypeOptions(TypeSpeed.Slow));

							await nextButton.ClickAsync(_puppeteerHandler.DoubleClick);
						}

						var sugestionTask = page.QuerySelectorAsync("#usernameList");
						phoneNumberBoxTask = page.WaitForSelectorAsync("#phoneNumberId");
						Task.WaitAny(sugestionTask, phoneNumberBoxTask);

						if (phoneNumberBoxTask.IsCompleted)
						{
							goto breakRegion;
						}

						await sugestionTask.Result.ClickAsync();
						await nextButton.ClickAsync(_puppeteerHandler.DoubleClick);
						await Task.Delay(TimeSpan.FromSeconds(4));
						phoneNumberBoxTask = page.WaitForSelectorAsync("#phoneNumberId");
						Task.WaitAll(phoneNumberBoxTask);

						breakRegion:

						#endregion

						#region Phone Verify Section

						var phoneNumberBox = phoneNumberBoxTask.Result;
						if (phoneNumberBox == null)
							return null;
						RetryAnotherNumber:
						await Task.Delay(TimeSpan.FromSeconds(2.5));
						var smsService = SmsFactory.Create();
						var issue = await smsService.IssueNumber();

						if (issue == null || string.IsNullOrEmpty(issue.Number)) return createdAccount;
						person.PhoneNumber = issue.Number;

						await phoneNumberBox.ClickAsync();
						for (var i = 0; i < person.PhoneNumber.Length; i++)
							await page.Keyboard.PressAsync("Backspace");

						await Task.Delay(TimeSpan.FromSeconds(1.5));
						await phoneNumberBox.TypeAsync(person.PhoneNumber, _puppeteerHandler.GetTypeOptions());

						var nextPhoneButton = await page.WaitForSelectorAsync("#gradsIdvPhoneNext");
						await nextPhoneButton.ClickAsync();

						await Task.Delay(TimeSpan
							.FromSeconds(
								10)); //there could be an error from google to say that the number is invalid (so will need to issue another number)

						try
						{
							var isNotValidPhoneNumberLabel =
								await page.WaitForXPathAsync(
									"//div[text()='This phone number cannot be used for verification.']",
									new WaitForSelectorOptions
									{
										Visible = true,
										Timeout = (int) TimeSpan.FromSeconds(3).TotalMilliseconds
									});
							goto RetryAnotherNumber;
						}
						catch
						{
							//continue
						}

						var codeSec = await page.WaitForSelectorAsync("#code");
						var codSecBounding = await codeSec.BoundingBoxAsync();
						await page.Mouse.MoveAsync(codSecBounding.X, codSecBounding.Y);

						var code = await smsService.GetVerificationCode(issue.Tzid);
						if (code == null)
							goto RetryAnotherNumber;
						await codeSec.TypeAsync(code.Message, _puppeteerHandler.GetTypeOptions());

						await Task.Delay(TimeSpan.FromSeconds(_random.Next(1, 4)));

						await page.Keyboard.PressAsync("Enter");

						//var veryButton = await page.WaitForSelectorAsync("#gradsIdvVerifyNext");
						//var veryBounding = await veryButton.BoundingBoxAsync();
						//await page.Mouse.MoveAsync(veryBounding.X, veryBounding.Y);
						//await veryButton.ClickAsync();

						#endregion

						await Task.Delay(TimeSpan.FromSeconds(7));

						#region Last Bits (Personal Information)

						var month = await page
							.WaitForSelectorAsync("#month"); //#day //#year #gender(select) //#personalDetailsNext
						await month.ClickAsync();

						await page.Keyboard.PressAsync("ArrowDown");
						await Task.Delay(TimeSpan.FromSeconds(1));
						await page.Keyboard.PressAsync("Enter");

						var day = await page.WaitForSelectorAsync("#day");
						await day.TypeAsync(_random.Next(0, 20).ToString(),
							_puppeteerHandler.GetTypeOptions(TypeSpeed.Fast));

						var year = await page.WaitForSelectorAsync("#year");
						await year.TypeAsync($"19{_random.Next(8, 9)}{_random.Next(0, 6)}",
							_puppeteerHandler.GetTypeOptions(TypeSpeed.Fast));

						var gender = await page.WaitForSelectorAsync("#gender");
						await gender.ClickAsync();

						await page.Keyboard.PressAsync("ArrowDown");
						await Task.Delay(TimeSpan.FromSeconds(1));
						await page.Keyboard.PressAsync("Enter");

						var fButton = await page.WaitForSelectorAsync("#personalDetailsNext");
						await fButton.ClickAsync();

						#endregion

						await Task.Delay(TimeSpan.FromSeconds(5));

						var skipButton = await page.WaitForXPathAsync("//button[text()='Skip']");
						await skipButton.ClickAsync();

						await Task.Delay(TimeSpan.FromSeconds(6));

						await page.Mouse.WheelAsync(10, 7000);
						await Task.Delay(TimeSpan.FromSeconds(1.24));

						await page.WaitForFunctionAsync(ScrollInsideDiv("eLUXld", 8000));
						await Task.Delay(TimeSpan.FromSeconds(1.12));

						//var downButton = await page.WaitForXPathAsync("//div[@class='U26fgb mUbCce fKz7Od erm3Qe M9Bg4d']");

						var checkBox1 = await page.QuerySelectorAsync("#c0");
						await checkBox1.ClickAsync();
						var checkBox2 = await page.QuerySelectorAsync("#c2");
						await checkBox2.ClickAsync();
						var termsOfServiceNextButton = await page.QuerySelectorAsync("#termsofserviceNext");
						await termsOfServiceNextButton.ClickAsync();
						await Task.Delay(TimeSpan.FromSeconds(2));
						var confirmButton = await page.QuerySelectorAsync("#confirmdialog-confirm");
						await confirmButton.ClickAsync();
						await Task.Delay(TimeSpan.FromSeconds(35));

						createdAccount = new EmailAccount
						{
							Topic = person.Topic,
							CreationDate = DateTime.UtcNow,
							UsedBy = new List<UsedBy>(),
							Person = new PersonModel
							{
								Email = person.PossibleEmails[pos],
								FirstName = person.FirstName,
								LastName = person.LastName,
								Gender = person.Gender,
								PossibleUsernames = person.PossibleUsernames,
								Password = person.Password,
								PhoneNumber = person.PhoneNumber,
								Proxy = person.Proxy
							},
						};
						await _gmailRepository.AddAccount(createdAccount);
					}
					catch (Exception err)
					{
						Console.WriteLine(err.Message);
					}
				}
			}

			return createdAccount;
		}

		public async Task<List<GmailMessagesResponse>> GetEmailBySubjectName(Browser browser,
			GmailLoginRequest loginRequest, ISearchRequest searchRequest)
		{
			var results = new List<GmailMessagesResponse>();
			if (browser == null)
			{
				browser = await _puppeteerHandler.OpenBrowser();
			}

			using (var page = await browser.NewPageAsync())
			{
				await page.InjectAntiScript();
				await page.SetUserAgentAsync(loginRequest.UserAgent);
				await page.GoToAsync(GMAIL_LOGIN_URL);

				#region Login To Gmail Account
				var userName = await page.WaitForElement("#identifierId", TimeSpan.FromSeconds(5));
				await userName.TypeAsync(loginRequest.Email, _puppeteerHandler.GetTypeOptions());
				var nextButton = await page.WaitForElement("#identifierNext", TimeSpan.FromSeconds(5));

				await nextButton.ClickAsync();
				await Task.Delay(TimeSpan.FromSeconds(1));

				var passElement = await page.WaitForElement("input[name=password]", TimeSpan.FromSeconds(5));
				await passElement.TypeAsync(loginRequest.Password, _puppeteerHandler.GetTypeOptions(TypeSpeed.Slow));

				await Task.Delay(TimeSpan.FromSeconds(.5));
				var passNextButton = await page.WaitForElement("#passwordNext", TimeSpan.FromSeconds(5));
				await passNextButton.ClickAsync();
				await Task.Delay(TimeSpan.FromSeconds(1));

				//U26fgb O0WRkf zZhnYe C0oVfc Zrq4w NTcbm RHAxtb M9Bg4d CONFIRM BUTTON

				var exists = await page.WaitXPathForElement("//div[@class='T-I J-J5-Ji T-I-KE L3']", TimeSpan.FromSeconds(5));

				// if this does not exist then the login has failed
				if (exists == null)
					return results;
				#endregion

				var names = await page.QuerySelectorAllAsync(".zA");
				await Task.Delay(TimeSpan.FromSeconds(1.2));
				foreach (var elementHandle in names)
				{
					try
					{
						var subjectElement = await elementHandle.QuerySelectorAsync(".yX.xY");
						var gridCell = await subjectElement.QuerySelectorAsync(".afn");
						var ba = await gridCell.QuerySelectorAsync(".bA4");
						var innerSpan = await ba.QuerySelectorAsync("span");
						var inner = await innerSpan.GetPropertyAsync("innerHTML");

						//if not found the subject go to the next one
						if (!inner.ToString().Split(':')[1].ToLower().Equals(searchRequest.SubjectName.ToLower())) continue;

						//otherwise click on the message
						await elementHandle.ClickAsync();
						await Task.Delay(TimeSpan.FromSeconds(1.5));

						//select the inner body of the email
						var innerContent =
							await page.WaitXPathForElement("//div[@class='ii gt']", TimeSpan.FromSeconds(4));

						//if it is null then just send back what it was found
						if (innerContent != null)
						{
							var body = await innerContent.GetPropertyAsync("innerHTML");

							if (searchRequest.VerifyTemplate.Verify)
							{
								var links = new Regex(searchRequest.VerifyTemplate.VerifyUrlRegexTemplate)
									.Matches(body.ToString());

								results.AddRange(links.DistinctBy(_ => _.ToString()).Select(_ => new GmailMessagesResponse(_.ToString(), BodyType.VerifyLink)));

								if (body.ToString().Contains(searchRequest.VerifyTemplate.VerifyCodeTemplate))
								{
									var code = new Regex(searchRequest.VerifyTemplate.VerifyCodeRegex).Match(body.ToString());
									results.Add(new GmailMessagesResponse(code.ToString(), BodyType.EmailVerifyCode));
								}
							}
							if (body.ToString().Contains(searchRequest.LoginNotices.LoginNoticeMessage)
								&& !searchRequest.LoginNotices.IgnoreLoginNotices)
								results.Add(new GmailMessagesResponse(body.ToString(), BodyType.LoginNotice));
							await page.GoBackAsync();
							continue;
						}

						var content = await page.GetContentAsync();
						results.Add(new GmailMessagesResponse(content, BodyType.Other));

						if (searchRequest.StopOnFirst && results.Count > 0)
							return results;

						await page.GoBackAsync();
					}
					catch (Exception err)
					{
						Console.WriteLine(err);
					}
				}
			}

			return results;
		}
		public async Task<List<GmailMessagesResponse>> GetInstagramMessages(GmailLoginRequest gmailLoginRequest,
			bool stopOnFirst = true, bool verify = true, bool ignoreLoginNotices = true)
		{
			if (gmailLoginRequest.UserAgent == null)
				gmailLoginRequest.UserAgent = OtherConstants.DEFAULT_USER_AGENT;
			var messages = new List<GmailMessagesResponse>();
			using (var browser = await _puppeteerHandler.OpenBrowser())
			{
				messages = await GetEmailBySubjectName(browser, gmailLoginRequest, new InstagramSearchRequest
				{
					SubjectName = "me",
					StopOnFirst = stopOnFirst,
					VerifyTemplate = new VerifyTemplate
					{
						Verify = verify,
						VerifyUrlRegexTemplate = "href=\"\\w*:*(\\/\\/)*(instagram.com)+[\\/|\\w*|?|=|&|;]*\"",
						VerifyCodeTemplate = "Someone tried to log in to your Instagram account.",
						VerifyCodeRegex = @"\d{6}"
					},
					LoginNotices = new LoginNotices
					{
						IgnoreLoginNotices = ignoreLoginNotices,
						LoginNoticeMessage = "We Noticed a New Login"
					}
				});
			}

			return messages;
		}
		public async Task<List<GmailMessagesResponse>> GetInstagramMessages(Browser browser, GmailLoginRequest gmailLoginRequest, 
			bool stopOnFirst = true, bool verify = true, bool ignoreLoginNotices = true)
		{
			if (gmailLoginRequest.UserAgent == null)
				gmailLoginRequest.UserAgent = OtherConstants.DEFAULT_USER_AGENT;

			var messages = await GetEmailBySubjectName(browser, gmailLoginRequest, new InstagramSearchRequest
			{
				SubjectName = "instagram",
				StopOnFirst = stopOnFirst,
				VerifyTemplate = new VerifyTemplate
				{
					Verify = verify,
					VerifyUrlRegexTemplate = "href=\"\\w*:*(\\/\\/)*(instagram.com)+[\\/|\\w*|?|=|&|;]*\"",
					VerifyCodeTemplate = "Someone tried to log in to your Instagram account.",
					VerifyCodeRegex = @"\d{6}"
				},
				LoginNotices = new LoginNotices
				{
					IgnoreLoginNotices = ignoreLoginNotices,
					LoginNoticeMessage = "We Noticed a New Login"
				}
			});
			return messages;
		}

		public async Task<string> AddServiceToEmail(string emailId, UsedBy usedBy)
			=> await _gmailRepository.AddService(emailId, usedBy);
		
		public async Task<bool> UpdateExistingService(string emailId, string serviceId, UsedBy usedBy) 
			=> await _gmailRepository.UpdateExistingService(emailId, serviceId, usedBy);

		public async Task<bool> RemoveEmailService(string emailId, string serviceId)
			=> await _gmailRepository.RemoveService(emailId, serviceId);

		public async Task<List<EmailAccount>> GetAllEmailAccounts()
			=> await _gmailRepository.GetAllEmailAccounts();
		public async Task<List<EmailAccount>> GetUnusedEmailAccounts(ByService notUsedByService)
			=> await _gmailRepository.GetUnusedAccounts(notUsedByService);
	}
}
