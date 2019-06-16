using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using System;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.CollectionsLogic
{
	public class CollectionsLogic : ICollectionsLogic
	{
		private readonly IAPIClientContainer Client;
		private readonly IReportHandler _reportHandler;

		public CollectionsLogic(IAPIClientContainer clientContexter, IReportHandler reportHandler)
		{
			Client = clientContexter;
			_reportHandler = reportHandler;
		}

		public async Task<IResult<InstaCollections>> GetCollections(int limit)
		{
			try { 

				return await Client.Collections.GetCollectionsAsync(PaginationParameters.MaxPagesToLoad(limit));
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaCollectionItem>> GetCollection(long collectionId, int limit)
		{
			try
			{
				return await Client.Collections.GetSingleCollectionAsync(collectionId, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaCollectionItem>> AddItemsCollection(long collectionId, params string[] mediaIds)
		{
			try { 
				return await Client.Collections.AddItemsToCollectionAsync(collectionId, mediaIds);
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaCollectionItem>> CreateCollection(string collectionName)
		{
			try { 
				return await Client.Collections.CreateCollectionAsync(collectionName);
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<bool>> DeleteCollection(long collectionId)
		{
			try { 
				return await Client.Collections.DeleteCollectionAsync(collectionId);
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}
		public async Task<IResult<InstaCollectionItem>> CreateCollection(long collectionId, string collectionName, string photoCoverId = null)
		{
			try { 
				return await Client.Collections.EditCollectionAsync(collectionId, collectionName, photoCoverId);
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee.Message);
				return null;
			}
		}

	}
}
