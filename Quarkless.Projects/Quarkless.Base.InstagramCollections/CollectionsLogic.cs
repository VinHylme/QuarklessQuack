using System;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.ReportHandler.Interfaces;

namespace Quarkless.Base.InstagramCollections
{
	public class CollectionsLogic : ICollectionsLogic
	{
		private readonly IApiClientContainer Client;
		private readonly IReportHandler _reportHandler;

		public CollectionsLogic(IApiClientContainer clientContexter, IReportHandler reportHandler)
		{
			Client = clientContexter;
			_reportHandler = reportHandler;
		}

		public async Task<IResult<InstaCollections>> GetCollections(int limit)
		{
			try { 

				return await Client.Collections.GetCollectionsAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch(Exception err)
			{
				await _reportHandler.MakeReport(err.Message);
				return null;
			}
		}
		public async Task<IResult<InstaCollectionItem>> GetCollection(long collectionId, int limit)
		{
			try
			{
				return await Client.Collections.GetSingleCollectionAsync(collectionId, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception err)
			{
				await _reportHandler.MakeReport(err.Message);
				return null;
			}
		}
		public async Task<IResult<InstaCollectionItem>> AddItemsCollection(long collectionId, params string[] mediaIds)
		{
			try { 
				return await Client.Collections.AddItemsToCollectionAsync(collectionId, mediaIds);
			}
			catch(Exception err)
			{
				await _reportHandler.MakeReport(err.Message);
				return null;
			}
		}
		public async Task<IResult<InstaCollectionItem>> CreateCollection(string collectionName)
		{
			try { 
				return await Client.Collections.CreateCollectionAsync(collectionName);
			}
			catch(Exception err)
			{
				await _reportHandler.MakeReport(err.Message);
				return null;
			}
		}
		public async Task<IResult<bool>> DeleteCollection(long collectionId)
		{
			try { 
				return await Client.Collections.DeleteCollectionAsync(collectionId);
			}
			catch(Exception err)
			{
				await _reportHandler.MakeReport(err.Message);
				return null;
			}
		}
		public async Task<IResult<InstaCollectionItem>> CreateCollection(long collectionId, string collectionName, string photoCoverId = null)
		{
			try { 
				return await Client.Collections.EditCollectionAsync(collectionId, collectionName, photoCoverId);
			}
			catch(Exception err)
			{
				await _reportHandler.MakeReport(err.Message);
				return null;
			}
		}

	}
}
