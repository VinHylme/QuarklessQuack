using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.Models.Business;
using InstagramApiSharp.Enums;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;

namespace QuarklessLogic.Logic.BusinessLogic
{
	public class BusinessLogic : IBusinessLogic
	{
		private IReportHandler _reportHandler { get; set; }
		private readonly IAPIClientContainer _client;

		public BusinessLogic(IReportHandler reportHandler, IAPIClientContainer client)
		{
			_reportHandler = reportHandler;
			_client = client;
			var account = _client?.GetContext?.InstagramAccount;
			if(account!=null)
				_reportHandler.SetupReportHandler(nameof(BusinessLogic), account.AccountId, account.Id);
			else
			{
				_reportHandler.SetupReportHandler(nameof(BusinessLogic));
			}
		}

		public async Task<IResult<InstaBusinessUser>> AddOrChangeBusinessButtonAsync(InstaBusinessPartner businessPartner, Uri uri)
		{
			try
			{
				return await _client.Business.AddOrChangeBusinessButtonAsync(businessPartner, uri);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBrandedContent>> AddUserToBrandedWhiteListAsync(params long[] userIdsToAdd)
		{
			try
			{
				return await _client.Business.AddUserToBrandedWhiteListAsync(userIdsToAdd);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBrandedContent>> DisbaleBrandedContentApprovalAsync()
		{
			try
			{
				return await _client.Business.DisbaleBrandedContentApprovalAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBrandedContent>> EnableBrandedContentApprovalAsync()
		{
			try
			{
				return await _client.Business.EnableBrandedContentApprovalAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBusinessUser>> ChangeBusinessCategoryAsync(string subCategoryId)
		{
			try
			{
				return await _client.Business.ChangeBusinessCategoryAsync(subCategoryId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaAccountDetails>> GetAccountDetailsAsync(long userId)
		{
			try
			{
				return await _client.Business.GetAccountDetailsAsync(userId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaUserInfo>> GetBusinessAccountInformationAsync()
		{
			try
			{
				return await _client.Business.GetBusinessAccountInformationAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBusinessPartnersList>> GetBusinessPartnersButtonsAsync()
		{
			try
			{
				return await _client.Business.GetBusinessPartnersButtonsAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBusinessCategoryList>> GetCategoriesAsync()
		{
			try
			{
				return await _client.Business.GetCategoriesAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaFullMediaInsights>> GetFullMediaInsightsAsync(string mediaId)
		{
			try
			{
				return await _client.Business.GetFullMediaInsightsAsync(mediaId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaMediaInsights>> GetMediaInsightsAsync(string mediaPk)
		{
			try
			{
				return await _client.Business.GetMediaInsightsAsync(mediaPk);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaMediaList>> GetPromotableMediaFeedsAsync()
		{
			try
			{
				return await _client.Business.GetPromotableMediaFeedsAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaStatistics>> GetStatisticsAsync()
		{
			try
			{
				return await _client.Business.GetStatisticsAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBusinessCategoryList>> GetSubCategoriesAsync(string categoryId)
		{
			try
			{
				return await _client.Business.GetSubCategoriesAsync(categoryId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBusinessSuggestedCategoryList>> GetSuggestedCategoriesAsync()
		{
			try
			{
				return await _client.Business.GetSuggestedCategoriesAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBrandedContent>> GetBrandedContentApprovalAsync()
		{
			try
			{
				return await _client.Business.GetBrandedContentApprovalAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBusinessUser>> RemoveBusinessButtonAsync()
		{
			try
			{
				return await _client.Business.RemoveBusinessButtonAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBusinessUser>> RemoveBusinessLocationAsync()
		{
			try
			{
				return await _client.Business.RemoveBusinessLocationAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBusinessCityLocationList>> SearchCityLocationAsync(string cityOrTown)
		{
			try
			{
				return await _client.Business.SearchCityLocationAsync(cityOrTown);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaDiscoverSearchResult>> SearchBrandedUsersAsync(string query, int count = 85)
		{
			try
			{
				return await _client.Business.SearchBrandedUsersAsync(query, count);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> StarDirectThreadAsync(string threadId)
		{
			try
			{
				return await _client.Business.StarDirectThreadAsync(threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> UnStarDirectThreadAsync(string threadId)
		{
			try
			{
				return await _client.Business.UnStarDirectThreadAsync(threadId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBrandedContent>> RemoveUserFromBrandedWhiteListAsync(params long[] userIdsToRemove)
		{
			try
			{
				return await _client.Business.RemoveUserFromBrandedWhiteListAsync(userIdsToRemove);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaBusinessUser>> UpdateBusinessInfoAsync(string phoneNumberWithCountryCode, InstaBusinessCityLocation cityLocation,
			string streetAddress, string zipCode, InstaBusinessContactType? businessContactType)
		{
			try
			{
				return await _client.Business.UpdateBusinessInfoAsync(phoneNumberWithCountryCode, cityLocation,
					streetAddress, zipCode, businessContactType);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> ValidateUrlAsync(InstaBusinessPartner desirePartner, Uri uri)
		{
			try
			{
				return await _client.Business.ValidateUrlAsync(desirePartner, uri);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
	}
}
