using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using MoreLinq;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using Quarkless.EmailServices.Logic.GmailService;
using Quarkless.EmailServices.Models;
using Quarkless.EmailServices.Models.Enums;
using Quarkless.InstagramCreator.Models;
using Quarkless.InstagramCreator.Models.Enums;
using Quarkless.InstagramCreator.Repository;
using Quarkless.PuppeteerClient.Logic;
using Quarkless.PuppeteerClient.Models.Enums;
using Quarkless.PuppeteerClient.Models.Extensions;
using Quarkless.SmsHandler.Logic;
using Quarkless.SmsHandler.Models.Enums;
using Quarkless.Utilities.Models.Constants;
using Quarkless.Utilities.Models.Extensions;
using Quarkless.Utilities.Models.Person;

namespace Quarkless.InstagramCreator.Logic
{
	public class InstagramService : IInstagramService
	{
		#region Initialiser
		private readonly IGmailClient _gmailClient;
		private readonly IInstagramAccountCreatorRepository _instagramAccountsRepository;
		private readonly PuppeteerHandler _puppeteerHandler;
		private readonly Random _random;
		#endregion
		#region Constants
		private const string BASE_URL = "https://www.instagram.com/";
		private const string USE_ENGLISH_ON_SITE = "?hl=en";
		private const string EDIT_PROFILE_URL = BASE_URL + "accounts/edit/";
		private const string LOGIN_INSTAGRAM_URL = BASE_URL + "accounts/login/?source=auth_switcher";
		#endregion

		public InstagramService(IGmailClient gmailClient, IInstagramAccountCreatorRepository instagramAccountsRepository)
		{
			_gmailClient = gmailClient;
			_instagramAccountsRepository = instagramAccountsRepository;
			_puppeteerHandler = new PuppeteerHandler();
			_random = new Random(Environment.TickCount);
		}

		public async Task<bool> LoginInstagram(Browser browser, string username, string password)
		{
			using (var page = await browser.NewPageAsync())
			{
				await page.InjectAntiScript();
				await page.GoToAsync(LOGIN_INSTAGRAM_URL);
				var usernameInput = await page.WaitForSelectorAsync("input[name=username]");
				await Task.Delay(TimeSpan.FromSeconds(0.34));
				await usernameInput.TypeAsync(username, _puppeteerHandler.GetTypeOptions());
				await Task.Delay(TimeSpan.FromSeconds(.8));
				var passwordInput = await page.WaitForSelectorAsync("input[name=password]");
				await Task.Delay(TimeSpan.FromSeconds(0.34));
				await passwordInput.TypeAsync(password, _puppeteerHandler.GetTypeOptions());
				await Task.Delay(TimeSpan.FromSeconds(0.6));
				var loginButton = await page.WaitForXPathAsync("//div[text()='Log In']");
				await Task.Delay(TimeSpan.FromSeconds(.4));
				await loginButton.ClickAsync();
				await Task.Delay(TimeSpan.FromSeconds(4));
				try
				{
					var existsField = await page.WaitForSelectorAsync(".XrOey");
					return true;
				}
				catch
				{
					return false;
				}
			}
		}
		public async Task<bool> ClickVerifyEmailButton(Browser browser)
		{
			using (var page = await browser.NewPageAsync())
			{
				await page.InjectAntiScript();
				await page.GoToAsync(EDIT_PROFILE_URL);
				await Task.Delay(TimeSpan.FromSeconds(4));
				try
				{
					//pepEmail email field input id
					var confirmButton = await page.WaitForXPathAsync("//button[text()='Confirm Email']",
						new WaitForSelectorOptions
						{
							Timeout = 2250
						});
					await Task.Delay(TimeSpan.FromSeconds(1.2));
					await confirmButton.ClickAsync();
					await Task.Delay(TimeSpan.FromSeconds(1.5));
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		public async Task StartTest(InstagramAccount person)
		{
			using (var browser = await _puppeteerHandler.OpenBrowser())
			{
				browser.Closed += (o, e) =>
				{
					_puppeteerHandler.RemoveBrowserContext(browser);
				};
				var login = await LoginInstagram(browser, person.Username, person.Password);
				if (login)
				{
					await BehaviourLoop(browser, person);
				}
			}
		}

		public async Task<List<InstagramAccount>> GetInstagramAccounts()
		{
			return await _instagramAccountsRepository.GetAllInstagramAccounts();
		}
		public async Task<InstagramAccountCreationEnum> StartService(EmailAccount emailAccount)
		{
			var addedServiceId = _gmailClient.AddServiceToEmail(emailAccount._id, new UsedBy
			{
				By = (int) ByService.Instagram
			}).Result;

			if (string.IsNullOrEmpty(addedServiceId))
				return InstagramAccountCreationEnum.Failed;
			InstagramAccount account = null;
			using (var browser = await _puppeteerHandler.OpenBrowser())
			{
				browser.Closed += (o, e) =>
				{
					_puppeteerHandler.RemoveBrowserContext(browser);
				};
				var createLogic = await CreateInstagramAccount(emailAccount);
				await Task.Delay(TimeSpan.FromSeconds(1));

				switch (createLogic.State)
				{
					case InstagramAccountCreationEnum.Failed:
						await _gmailClient.UpdateExistingService(emailAccount._id, addedServiceId, 
							new UsedBy
						{
							_id = addedServiceId,
							By = (int) ByService.Instagram,
							HasFailed = true
						});
						return InstagramAccountCreationEnum.Failed;
					case InstagramAccountCreationEnum.ExceptionThrown:
						await _gmailClient.RemoveEmailService(emailAccount._id, addedServiceId);
						return InstagramAccountCreationEnum.ExceptionThrown;
					case InstagramAccountCreationEnum.CouldNotFetchOrVerifyNumber:
						await _gmailClient.RemoveEmailService(emailAccount._id, addedServiceId);
						return InstagramAccountCreationEnum.CouldNotFetchOrVerifyNumber;
					case InstagramAccountCreationEnum.NoUsernamesFound:
						await _gmailClient.RemoveEmailService(emailAccount._id, addedServiceId);
						return InstagramAccountCreationEnum.NoUsernamesFound;
					case InstagramAccountCreationEnum.Created:
						await _gmailClient.UpdateExistingService(emailAccount._id, addedServiceId, 
							new UsedBy
						{
							_id = addedServiceId,
							By = (int) ByService.Instagram,
							HasFailed = false,
							Virgin = true
						});
						account = createLogic.Account;
						if(account!=null)
							await _instagramAccountsRepository.AddAccount(account);
						break;
				}

				if (account == null)
					return InstagramAccountCreationEnum.ExceptionThrown;

				#region Verify Account Section
				var confirmLogic = await ClickVerifyEmailButton(browser);
				await Task.Delay(TimeSpan.FromSeconds(1));
				if (!confirmLogic)
				{
					account.NeedsVerifying = true;
					goto EndPhase;
				}

				const int maxIteration = 3;
				var currentIteration = 0;

				TryAgain:
				if(currentIteration > maxIteration)
					goto EndPhase;

				await Task.Delay(TimeSpan.FromSeconds(8));
				var messages = await _gmailClient.GetInstagramMessages(browser,
					new GmailLoginRequest(emailAccount.Person.Email, emailAccount.Person.Password));
				var code = messages.FirstOrDefault(_ => _.BodyType == BodyType.VerifyLink);
				if (code == null)
				{
					currentIteration++;
					goto TryAgain;
				}
				
				using (var page = await browser.NewPageAsync())
				{
					await page.InjectAntiScript();
					await page.GoToAsync(Regex.Match(code.Body, "\".*\"").Value.Replace("\"", ""));
					await Task.Delay(TimeSpan.FromSeconds(8));
				}
				#endregion

				EndPhase:
				return InstagramAccountCreationEnum.Created;
			}
		}
		public async Task<InstagramAccountCreationEnum> StartService(PersonCreateModel person)
		{
			var proxyLine = person.Proxy != null
				? $"--proxy-server=http://{person.Proxy.IP}:{person.Proxy.Port}"
				: null;
			
			using (var browser = await _puppeteerHandler.OpenBrowser(proxyLine))
			{
				browser.Closed += (o, e) =>
				{
					_puppeteerHandler.RemoveBrowserContext(browser);
				};

				var createLogic = await CreateInstagramAccountUsingPhone(browser, person);
				if (createLogic == null)
					return InstagramAccountCreationEnum.ExceptionThrown;
				switch (createLogic.State)
				{
					case InstagramAccountCreationEnum.Failed:
						return InstagramAccountCreationEnum.Failed;
					case InstagramAccountCreationEnum.ExceptionThrown:
						return InstagramAccountCreationEnum.ExceptionThrown;
					case InstagramAccountCreationEnum.CouldNotFetchOrVerifyNumber:
						return InstagramAccountCreationEnum.CouldNotFetchOrVerifyNumber;
					case InstagramAccountCreationEnum.NoUsernamesFound:
						return InstagramAccountCreationEnum.NoUsernamesFound;
					case InstagramAccountCreationEnum.Created:
						if (createLogic.Account != null)
							await _instagramAccountsRepository.AddAccount(createLogic.Account);
						break;
					default: return InstagramAccountCreationEnum.ExceptionThrown;
				}

				if (createLogic.Account != null)
				{
					await BehaviourLoop(browser, createLogic.Account);
				}

				return createLogic.State;
			}
			
		}

		private async Task<bool> BehaviourLoop(Browser browser, InstagramAccount person)
		{
			var repeat = _random.Next(2,4);
			var current = 0;
			using (var page = await browser.NewPageAsync())
			{
				await page.GoToAsync(BASE_URL);

				#region Notification
				async Task<bool> FindNotificationButtonAndPress()
				{
					try
					{
						var buttonTurnOnNotification = await page.WaitForXPathAsync(
							"//button[text()='Turn On']",
							new WaitForSelectorOptions { Timeout = 10000 });
						await buttonTurnOnNotification.ClickAsync(new ClickOptions { ClickCount = 1 });
						Console.WriteLine("AccountCreated");
						return true;
					}
					catch
					{
						Console.WriteLine("Failed to create account");

						return false;
					}
				}
				#endregion

				await FindNotificationButtonAndPress();

				async Task<bool> FollowUser()
				{
					var shouldDoAction = _random.Next(-2, 4) > 0;
					if (!shouldDoAction) return false;
					try
					{
						var followButton = await page.WaitForXPathAsync("//button[text()='Follow']", new WaitForSelectorOptions
						{
							Timeout = (int)TimeSpan.FromSeconds(4).TotalMilliseconds
						});
						await followButton.ClickAsync();
						await SpamDetectionWasActivated();
						return true;
					}
					catch(Exception err)
					{
						Console.WriteLine("Failed To follow user: {0}", err.Message);
						return false;
					}
				}
				async Task<bool> LikeMediaDuringView()
				{
					var shouldDoAction = _random.Next(-2, 4) > 0;
					if (!shouldDoAction) return false;
					try
					{
						var likeButton = await page.WaitForSelectorAsync(".fr66n");
//						var likeButton = await page.WaitForXPathAsync("//button[@class='dCJp8 afkep']", new WaitForSelectorOptions
//						{
//							Timeout = (int) TimeSpan.FromSeconds(4).TotalMilliseconds
//						});
						await Task.Delay(TimeSpan.FromSeconds(_random.Next(1, 2)));
						await likeButton.ClickAsync();
						await SpamDetectionWasActivated();
						return true;
					}
					catch(Exception err)
					{
						Console.WriteLine("Failed To like media: {0}", err.Message);
						return false;
					}
				}
				async Task<bool> CloseMediaView()
				{
					try
					{
						var closeButton = await page.WaitForXPathAsync("//button[text()='Close']");
						await closeButton.ClickAsync();
						return true;
					}
					catch(Exception err)
					{
						Console.WriteLine("Failed To close media: {0}", err.Message);
						return false;
					}
				}
				async Task<ElementHandle[]> GetMediasOnCurrentPage()
				{
					try
					{
						var postsContainers = await page.QuerySelectorAllAsync(".v1Nh3.kIKUG");
						return postsContainers;
					}
					catch
					{
						return new ElementHandle[] { };
					}
				}

				async Task<bool> UserViewActioning(int takeAmount)
				{
					var shouldDoAction = _random.Next(-2, 2) > 0;
					if (!shouldDoAction) return false;
					var completed = false;
					
					try
					{
						var userButton = await page.WaitForSelectorAsync(".e1e1d");
						await userButton.ClickAsync();
						await Task.Delay(TimeSpan.FromSeconds(5));

						var followRes = await FollowUser();

						var posts = await GetMediasOnCurrentPage();

						if (!posts.Any() && !followRes)
							goto EndTask;
						if (!posts.Any() && followRes)
							goto EndTask;

						await Task.Delay(TimeSpan.FromSeconds(1.4));
						
						foreach (var post in posts.Take(6).TakeAny(takeAmount, _random))
						{
							await page.Mouse.WheelAsync(0, 400);
							await Task.Delay(TimeSpan.FromSeconds(1.5));
							await post.ClickAsync();
							//await Task.Delay(TimeSpan.FromSeconds(2));
							//await LikeMediaDuringView();
							await Task.Delay(TimeSpan.FromSeconds(1.75));
							await CloseMediaView();
						}

						completed = true;
					}
					catch(Exception err)
					{
						Console.WriteLine(err.Message);
					}

					EndTask:
					//await GoBack();
					return completed;
				}

				async Task GoBack()
				{
					while (page.Url != BASE_URL + "explore/")
					{
						await page.GoBackAsync();
						await Task.Delay(TimeSpan.FromSeconds(.4));
					}
				}

				async Task<bool> SpamDetectionWasActivated()
				{
					try
					{
						await Task.Delay(TimeSpan.FromSeconds(1.5));
						var okButton = await page.WaitForXPathAsync("//button[text()='OK']", new WaitForSelectorOptions
						{
							Timeout = (int) TimeSpan.FromSeconds(2).TotalMilliseconds
						});
						await okButton.ClickAsync();
						return true;
					}
					catch
					{
						return false;
					}
				}

				async Task ExploreBrowse()
				{
					try
					{
						if (page.Url == BASE_URL + "explore/")
						{
							await page.ReloadAsync();
							await Task.Delay(TimeSpan.FromSeconds(3));
						}

						var exploreButton = await page.WaitForSelectorAsync(".XrOey");
						await exploreButton.ClickAsync();

						await Task.Delay(TimeSpan.FromSeconds(2));

						var postsContainer = await GetMediasOnCurrentPage();
						if (!postsContainer.Any())
							return;

						await page.Mouse.WheelAsync(0, _random.Next(20, 80));
						var post = postsContainer.Take(12).Shuffle().First();

						await post.ClickAsync();

						await Task.Delay(TimeSpan.FromSeconds(_random.Next(3, 4)));
						await LikeMediaDuringView();
						await Task.Delay(TimeSpan.FromSeconds(1.5));
						await FollowUser();
						await Task.Delay(TimeSpan.FromSeconds(_random.Next(1, 2)));
						await UserViewActioning(_random.Next(2,4));
						await Task.Delay(TimeSpan.FromSeconds(_random.Next(1, 2)));
					}
					catch (Exception err)
					{
						Console.WriteLine("Failed to browse explore, {0}", err.Message);
					}
				}

				try
				{
					await Task.Delay(TimeSpan.FromSeconds(2));

					while (current < repeat)
					{
						current++;
						await ExploreBrowse();
					}

					await Task.Delay(TimeSpan.FromSeconds(12));
					await page.GoToAsync(BASE_URL + person.Username);
					await Task.Delay(TimeSpan.FromSeconds(2));
					var cogButton = await page.WaitForSelectorAsync(".dCJp8.afkep");
					await cogButton.ClickAsync();
					await Task.Delay(TimeSpan.FromSeconds(0.7));
					var logoutButton = await page.WaitForXPathAsync("//button[text()='Log Out']");
					await logoutButton.ClickAsync();
					await Task.Delay(TimeSpan.FromSeconds(1));
				}
				catch(Exception err)
				{
					Console.WriteLine(err.Message);
				}
			}

			return true;
		}
		private async Task<InstagramAccountCreationResponse> CreateInstagramAccountUsingPhone(Browser browser, PersonCreateModel person)
		{
			var response = new InstagramAccountCreationResponse();
			using (var page = await browser.NewPageAsync())
			{
				try
				{
					await page.InjectAntiScript();
					await page.SetUserAgentAsync(person.UserAgent ?? OtherConstants.DEFAULT_USER_AGENT);
					if (person.Proxy != null)
						await page.AuthenticateAsync(new Credentials
							{Password = person.Proxy.Password, Username = person.Proxy.Username});

					await page.GoToAsync(BASE_URL + USE_ENGLISH_ON_SITE);

					#region Click on cookies notification

					try
					{
						var selector = ".KPZNL";
						var element = await page.WaitForSelectorAsync(selector, new WaitForSelectorOptions
						{
							Timeout = 1250
						});
						var boelement = await element.BoundingBoxAsync();
						await page.Mouse.MoveAsync(boelement.X, boelement.Y);
						await element.ClickAsync();
					}
					catch
					{
						//continue
					}

					#endregion

					#region Phone or Email Field

					var phoneNumberField = await page.WaitForSelectorAsync("input[name=emailOrPhone]");
					await phoneNumberField.ClickAsync();

					#endregion

					#region Phone Field
					var smsService = SmsFactory.Create(SmsServiceType.Instagram);

					var issue = await smsService.IssueNumberNew();

					if (issue == null || string.IsNullOrEmpty(issue.Number))
					{
						response.State = InstagramAccountCreationEnum.CouldNotFetchOrVerifyNumber;
						response.Account = null;
						return response;
					}

					person.PhoneNumber = issue.Number;
					await phoneNumberField.TypeAsync(person.PhoneNumber, _puppeteerHandler.GetTypeOptions());
					#endregion

					#region Full-Name Field
					var fullNameField = await page.WaitForSelectorAsync("input[name=fullName]");
					await fullNameField.TypeAsync(person.FirstName + " " + person.LastName, _puppeteerHandler.GetTypeOptions());
					await page.Keyboard.PressAsync("Tab");
					#endregion

					#region Username Field

					var pos = 0;
					var usernameField = await page.WaitForSelectorAsync("input[name=username]");
					var usernameClean = false;
					do
					{
						if (pos >= person.PossibleUsernames.Length)
						{
							response.State = InstagramAccountCreationEnum.NoUsernamesFound;
							response.Account = null;
							return response;
						}

						await usernameField.ClickAsync();
						await Task.Delay(TimeSpan.FromSeconds(.1));

						if (pos != 0)
						{
							await page.Keyboard.DownAsync("Control");
							await page.Keyboard.PressAsync("KeyA");
							await page.Keyboard.PressAsync("Backspace");
							await Task.Delay(TimeSpan.FromSeconds(.12));
							await page.Keyboard.UpAsync("Control");
						}

						await usernameField.TypeAsync(person.PossibleUsernames[pos],
							_puppeteerHandler.GetTypeOptions());

						await fullNameField.ClickAsync();
						try
						{
							var exists = await page.WaitForXPathAsync("//span[@class='coreSpriteInputError gBp1f']",
								new WaitForSelectorOptions
								{
									Visible = true,
									Timeout = (int) TimeSpan.FromSeconds(2).TotalMilliseconds
								});
							pos++;
							usernameClean = false;
						}
						catch
						{
							usernameClean = true;
						}
					} while (!usernameClean);

					#endregion

					#region Password Field

					var passwordField = await page.WaitForSelectorAsync("input[name=password]");
					await passwordField.TypeAsync(person.Password,
						_puppeteerHandler.GetTypeOptions(TypeSpeed.Medium));
					await page.Keyboard.PressAsync("Tab");
					#endregion

					#region Submit Button
					try
					{
						var submitButton = await page.WaitForXPathAsync("//button[text()='Next']",
							new WaitForSelectorOptions
							{
								Timeout = 2250
							});
						await submitButton.ClickAsync();
					}
					catch
					{
						var submitButton = await page.WaitForXPathAsync("//button[text()='Sign up']",
							new WaitForSelectorOptions
							{
								Timeout = 2250
							});
						var submitBounding = await submitButton.BoundingBoxAsync();
						await page.Mouse.MoveAsync(submitBounding.X, submitBounding.Y);
						await submitButton.ClickAsync();
					}

					#endregion

					#region Verify Phone Number
					var confirmPhoneField = await page.WaitForSelectorAsync("input[name=confirmationCode]");
					var confirmButton = await page.WaitForXPathAsync("//button[text()='Confirm']");

					//phoneSignupConfirmErrorAlert
					var timer = new Timer(TimeSpan.FromMinutes(2.25).TotalMilliseconds);
					timer.Start();
					timer.Elapsed += async(o, e) =>
					{
						try
						{
							var sendNewButton = await page.WaitForXPathAsync("//button[text()='Request New Code']");
							await sendNewButton.ClickAsync();
						}
						catch
						{
							//
						}
					};
					var code = await smsService.GetVerificationCode(issue.Tzid);

					if (code == null)
					{
						response.State = InstagramAccountCreationEnum.CouldNotFetchOrVerifyNumber;
						response.Account = null;
						return response;
					}

					await confirmPhoneField.TypeAsync(code.Message, _puppeteerHandler.GetTypeOptions());
					await Task.Delay(TimeSpan.FromSeconds(1));
					await confirmButton.ClickAsync();
					try
					{
						var error = await page.WaitForSelectorAsync("#phoneSignupConfirmErrorAlert", 
							new WaitForSelectorOptions
							{
								Timeout = (int) TimeSpan.FromSeconds(8).TotalMilliseconds
							});
						response.State = InstagramAccountCreationEnum.CouldNotFetchOrVerifyNumber;
						response.Account = null;
						return response;
					}
					catch
					{
						//continue;
					}
					#region Html
					//<input aria-required="true" aria-describedby="confirmationCodeDescription" aria-label="######"
					//autocapitalize="off" autocorrect="off" maxlength="6" name="confirmationCode" type="tel"
					//class="_2hvTZ pexuQ zyHYP" value="">
					//input name confirmationCode
					//<button class="sqdOP  L3NKy   y3zKF     " type="button">Confirm</button>
					//<button class="sqdOP yWX7d    y3zKF     " type="button">Change Number</button>
					//<button class="sqdOP yWX7d    y3zKF     " type="button">Request New Code</button>
					//input name newPhoneNumber
					#endregion

					#endregion

					#region Notification
					async Task<InstagramAccountCreationEnum> FindNotificationButtonAndPress()
					{
						try
						{
							var buttonTurnOnNotification = await page.WaitForXPathAsync(
								"//button[text()='Turn On']",
								new WaitForSelectorOptions
								{
									Timeout = 10000
								});
							await buttonTurnOnNotification.ClickAsync(new ClickOptions {ClickCount = 1});
							Console.WriteLine("AccountCreated");
							return InstagramAccountCreationEnum.Created;
						}
						catch
						{
							Console.WriteLine("Failed to create account");

							return InstagramAccountCreationEnum.Failed;
						}
					}
					#endregion

					#region Is Over 18 checkbox
					try
					{
						var buttonIsOver18 = await page.WaitForSelectorAsync("#igCoreRadioButtonageRadioabove_18", 
							new WaitForSelectorOptions
							{
								Timeout = (int)TimeSpan.FromSeconds(7.5).TotalMilliseconds
							});
						await buttonIsOver18.ClickAsync();
						await Task.Delay(TimeSpan.FromSeconds(.5));
						var finalSubmit = (await page.XPathAsync("//button[text()='Next']")).Last();
						await finalSubmit.ClickAsync();
						await Task.Delay(TimeSpan.FromSeconds(7));
						try
						{
							var captchaLabel = await page.WaitForSelectorAsync("#recaptcha-anchor-label", 
								new WaitForSelectorOptions{Timeout = (int) TimeSpan.FromSeconds(8).TotalMilliseconds});
							response.State = InstagramAccountCreationEnum.Failed;
							response.Account = null;
							return response;
						}
						catch
						{
							//continue
						}

						try
						{
							var failedLabel = await page.WaitForSelectorAsync("#phoneSignupConfirmErrorAlert",
								new WaitForSelectorOptions
									{Timeout = (int) TimeSpan.FromSeconds(8).TotalMilliseconds, Visible = true});
							response.State = InstagramAccountCreationEnum.Failed;
							response.Account = null;
							return response;
						}
						catch
						{
							//continue
						}


						await Task.Delay(TimeSpan.FromSeconds(5));
						response.State = InstagramAccountCreationEnum.Created;
						response.Account = new InstagramAccount
						{
							CreationTime = DateTime.UtcNow,
							Username = person.PossibleUsernames[pos],
							Email = null,
							FirstName = person.FirstName,
							LastName = person.LastName,
							Gender = person.Gender,
							Password = person.Password,
							PhoneNumber = person.PhoneNumber,
							Topic = person.Topic,
							Virgin = true
						};
						return response;
					}
					catch
					{
						Console.WriteLine(
							"no over 18 dialog showed, trying to see if user logged in automatically...");
						var state = await FindNotificationButtonAndPress();
							
						await Task.Delay(TimeSpan.FromSeconds(5));
						if (state == InstagramAccountCreationEnum.Created)
						{
							response.Account = new InstagramAccount
							{
								CreationTime = DateTime.UtcNow,
								Username = person.PossibleUsernames[pos],
								Email = null,
								FirstName = person.FirstName,
								LastName = person.LastName,
								Gender = person.Gender,
								Password = person.Password,
								PhoneNumber = person.PhoneNumber,
								Topic = person.Topic,
								Virgin = true
							};
						}
						else
						{
							response.Account = null;
						}

						response.State = state;
						return response;
					}
					#endregion
				}
				catch
				{
					response.State = InstagramAccountCreationEnum.ExceptionThrown;
					response.Account = null;
					return response;
				}
			}
		}
		public async Task<InstagramAccountCreationResponse> CreateInstagramAccount(EmailAccount emailAccount)
		{
			var response = new InstagramAccountCreationResponse();
			var person = emailAccount.Person;
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

						await page.GoToAsync(BASE_URL + USE_ENGLISH_ON_SITE);

						#region Click on cookies notification

						try
						{
							var selector = ".KPZNL";
							var element = await page.WaitForSelectorAsync(selector, new WaitForSelectorOptions
							{
								Timeout = 1250
							});
							await element.ClickAsync();
						}
						catch
						{
							//continue
						}

						#endregion

						#region Name Field
						var emailField = await page.WaitForSelectorAsync("input[name=emailOrPhone]");
						await emailField.ClickAsync();
						#endregion

						#region Email Field
						await emailField.TypeAsync(person.Email, _puppeteerHandler.GetTypeOptions());
						await page.Keyboard.PressAsync("Tab");
						#endregion

						#region Full-Name Field
						var fullNameField = await page.WaitForSelectorAsync("input[name=fullName]");
						await fullNameField.TypeAsync(person.FirstName + " " + person.LastName,
							_puppeteerHandler.GetTypeOptions());
						await page.Keyboard.PressAsync("Tab");
						#endregion

						#region Username Field

						var pos = 0;
						var usernameField = await page.WaitForSelectorAsync("input[name=username]");
						var usernameClean = false;
						do
						{
							if (pos >= person.PossibleUsernames.Length)
							{
								response.State = InstagramAccountCreationEnum.NoUsernamesFound;
								response.Account = null;
								return response;
							}

							await usernameField.ClickAsync();
							await Task.Delay(TimeSpan.FromSeconds(.1));

							if (pos != 0)
							{
								await page.Keyboard.DownAsync("Control");
								await page.Keyboard.PressAsync("KeyA");
								await page.Keyboard.PressAsync("Backspace");
								await Task.Delay(TimeSpan.FromSeconds(.12));
								await page.Keyboard.UpAsync("Control");
							}

							await usernameField.TypeAsync(person.PossibleUsernames[pos],
								_puppeteerHandler.GetTypeOptions());

							await fullNameField.ClickAsync();
							try
							{
								var exists = await page.WaitForXPathAsync("//span[@class='coreSpriteInputError gBp1f']",
									new WaitForSelectorOptions
									{
										Visible = true,
										Timeout = (int)TimeSpan.FromSeconds(2).TotalMilliseconds
									});
								pos++;
								usernameClean = false;
							}
							catch
							{
								usernameClean = true;
							}
						} while (!usernameClean);

						#endregion

						#region Password Field
						var passwordField = await page.WaitForSelectorAsync("input[name=password]");
						await passwordField.TypeAsync(person.Password, _puppeteerHandler.GetTypeOptions());
						#endregion

						#region Submit Button (first)
						try
						{
							var submitButton = await page.WaitForXPathAsync("//button[text()='Next']",
								new WaitForSelectorOptions
								{
									Timeout = 2250
								});
							await submitButton.ClickAsync();
						}
						catch
						{
							var submitButton = await page.WaitForXPathAsync("//button[text()='Sign up']",
								new WaitForSelectorOptions
								{
									Timeout = 2250
								});
							await submitButton.ClickAsync();
						}
						#endregion

						#region Notification
						async Task<InstagramAccountCreationEnum> FindNotificationButtonAndPress()
						{
							try
							{
								var buttonTurnOnNotification = await page.WaitForXPathAsync(
									"//button[text()='Turn On']",
									new WaitForSelectorOptions {Timeout = 12000});

								await buttonTurnOnNotification.ClickAsync(new ClickOptions {ClickCount = 2});
								Console.WriteLine("AccountCreated");
								return InstagramAccountCreationEnum.Created;
							}
							catch
							{
								Console.WriteLine("Failed to create account");
								return InstagramAccountCreationEnum.Failed;
							}
						}

						#endregion

						#region Is Over 18 checkbox
						try
						{
							var buttonIsOver18 = await page.WaitForSelectorAsync("#igCoreRadioButtonageRadioabove_18",
								new WaitForSelectorOptions
								{
									Timeout = 8000
								});
							await buttonIsOver18.ClickAsync(new ClickOptions {ClickCount = 2});
						}
						catch
						{
							Console.WriteLine(
								"no over 18 dialog showed, trying to see if user logged in automatically...");
							var state = await FindNotificationButtonAndPress();
							if (state == InstagramAccountCreationEnum.Created)
								response.Account = new InstagramAccount
								{
									CreationTime = DateTime.UtcNow,
									Email = emailAccount.Person.Email,
									FirstName = emailAccount.Person.FirstName,
									LastName = emailAccount.Person.LastName,
									Gender = emailAccount.Person.Gender,
									Password = emailAccount.Person.Password,
									PhoneNumber = null,
									Username = emailAccount.Person.PossibleUsernames[pos],
									Topic = emailAccount.Topic,
									Virgin = true
								};
							else
							{
								response.Account = null;
							}

							response.State = state;
							return response;
						}

						#endregion

						#region Final Submit Button
						var finalSubmit = (await page.XPathAsync("//button[text()='Next']")).Last();
						await finalSubmit.ClickAsync();
						try
						{
							var err = await page.WaitForSelectorAsync("#ssfErrorAlert",
								new WaitForSelectorOptions {Timeout = 9000});
							Console.WriteLine("Failed to create account");
							response.State = InstagramAccountCreationEnum.Failed;
							response.Account = null;
							return response;
						}
						catch
						{
							var state = await FindNotificationButtonAndPress();
							if (state == InstagramAccountCreationEnum.Created)
								response.Account = new InstagramAccount
								{
									CreationTime = DateTime.UtcNow,
									Email = emailAccount.Person.Email,
									FirstName = emailAccount.Person.FirstName,
									LastName = emailAccount.Person.LastName,
									Gender = emailAccount.Person.Gender,
									Password = emailAccount.Person.Password,
									PhoneNumber = null,
									Username = emailAccount.Person.PossibleUsernames[pos],
									Topic = emailAccount.Topic,
									Virgin = true
								};
							else
							{
								response.Account = null;
							}

							response.State = state;
							return response;
						}
						#endregion
					}
					catch
					{
						response.State = InstagramAccountCreationEnum.ExceptionThrown;
						response.Account = null;
						return response;
					}
				}
			}
		}
	}
}
