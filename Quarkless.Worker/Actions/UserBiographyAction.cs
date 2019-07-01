using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.Repository.ServicesRepositories;
using QuarklessRepositories.Repository.ServicesRepositories.UserBiographyRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quarkless.Worker.Actions
{
	public class UserBiographyAction : IWActions
	{
		private readonly string Topic;
		private readonly IAPIClientContainer context_;
		private readonly IReportHandler _reportHandler;
		private readonly IUserBiographyRepository _userBiographyRepository;
		public UserBiographyAction(IAPIClientContainer context, IReportHandler reportHandler, string topic, int limit, params IServiceRepository[] serviceRepository)
		{
			this.Topic = topic;
			this.context_ = context;
			this._reportHandler = reportHandler;
			foreach (var repo in serviceRepository)
			{
				if (repo is IUserBiographyRepository)
					this._userBiographyRepository = (IUserBiographyRepository)repo;
			}

			_reportHandler.SetupReportHandler("UserBiographyAction", context.GetContext.InstagramAccount.AccountId, context.GetContext.InstagramAccount.Username);
		}

		public Task<object> Operate()
		{
			throw new NotImplementedException();
		}

		public async Task<bool> Operate(List<object> medias)
		{
			try { 
			Console.WriteLine("STARTED BIO");
			List<UserBiographyModel> userBiographies = new List<UserBiographyModel>();
			for(int i = 0; i < medias.Count; i++)
			{
				InstaMedia media = (InstaMedia) medias.ElementAt(i);
				var user = media.User;
				var res = await context_.User.GetFullUserInfoAsync(user.Pk);
				if (res.Succeeded)
				{
					var bio = res.Value.UserDetail.Biography;
					var curUser = res.Value.UserDetail;
					userBiographies.Add(new UserBiographyModel 
					{ 
						Text = bio, 
						Topic = this.Topic, 
						User = new User
						{
							FollowerCount = curUser.FollowerCount,
							FollowingCount = curUser.FollowingCount,
							FullName = curUser.FullName,
							UserId = curUser.Pk,
							Username = curUser.UserName
						},
						Contact = new Contact
						{
							Address = curUser.AddressStreet,
							CityId = curUser.CityId,
							CityName = curUser.CityName,
							ContactEmail = curUser.PublicEmail,
							ContactPhone = new PhoneContact
							{
								ContactPhone = curUser.ContactPhoneNumber,
								CountryCode = curUser.PublicPhoneCountryCode,
								PublicPhone = curUser.PublicPhoneNumber
							}
						},
					});
				}
				else
				{
					if(res.Info.ResponseType == ResponseType.RequestsLimit || res.Info.ResponseType == ResponseType.NetworkProblem)
					{
						Console.WriteLine(res.Info.Message);
						return false;
					}
					else { 
						_reportHandler.MakeReport(res.Info);
						continue;
					}
				}
				//await Task.Delay(TimeSpan.FromSeconds(1.5));
			}
			_userBiographyRepository.AddBiographies(userBiographies);
			return true;
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				_reportHandler.MakeReport(ee);
				return false;
			}
		}

		public async Task<bool> Operate(object item)
		{
			try
			{
				Console.WriteLine("STARTED BIO");
				List<UserBiographyModel> userBiographies = new List<UserBiographyModel>();
				InstaMedia media = (InstaMedia)item;
				var user = media.User;
				var res = await context_.User.GetFullUserInfoAsync(user.Pk);
				if (res.Succeeded)
				{
					var bio = res.Value.UserDetail.Biography;
					var curUser = res.Value.UserDetail;
					userBiographies.Add(new UserBiographyModel
					{
						Text = bio,
						Topic = this.Topic,
						User = new User
						{
							FollowerCount = curUser.FollowerCount,
							FollowingCount = curUser.FollowingCount,
							FullName = curUser.FullName,
							UserId = curUser.Pk,
							Username = curUser.UserName
						},
						Contact = new Contact
						{
							Address = curUser.AddressStreet,
							CityId = curUser.CityId,
							CityName = curUser.CityName,
							ContactEmail = curUser.PublicEmail,
							ContactPhone = new PhoneContact
							{
								ContactPhone = curUser.ContactPhoneNumber,
								CountryCode = curUser.PublicPhoneCountryCode,
								PublicPhone = curUser.PublicPhoneNumber
							}
						},
					});
				}
				else
				{
					_reportHandler.MakeReport(res.Info);
					return false;
				}
				
				_userBiographyRepository.AddBiographies(userBiographies);
				return true;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				_reportHandler.MakeReport(ee);
				return false;
			}
		}
	}
}
