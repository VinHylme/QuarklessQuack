﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Enums;
using Quarkless.Models.Proxy.Interfaces;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Repository.Proxy
{
	public class ProxyAssignmentsRepository : IProxyAssignmentsRepository
	{
		private readonly IMongoCollection<ProxyModel> _ctx;
		private const string COLLECTION_NAME = "UserProxies";

		public ProxyAssignmentsRepository(IMongoClientContext context)
		 => _ctx = context.AccountDatabase.GetCollection<ProxyModel>(COLLECTION_NAME);

		public async Task<bool> AddAssignedProxy(ProxyModel proxy)
		{
			try
			{
				await _ctx.InsertOneAsync(proxy);
				return true;
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return false;
			}
		}
		public async Task<ProxyModel> ReassignProxy(string proxyId, ProxyModel newModel)
		{
			try
			{
				var filter = new FilterDefinitionBuilder<ProxyModel>().Eq("_id", proxyId);
				var updated = await _ctx.FindOneAndReplaceAsync(filter, newModel);
				return updated;
			}
			catch
			{
				return null;
			}
		}
		public async Task<bool> DeleteProxyAssigned(string proxyId)
		{
			try
			{
				await _ctx.FindOneAndDeleteAsync(Builders<ProxyModel>.Filter.Eq("_id", proxyId));
				return true;
			}
			catch
			{
				return false;
			}
		}

		public async Task<ProxyModel> GetProxyAssigned(string accountId, string instagramAccountId)
		{
			try
			{
				var builds = Builders<ProxyModel>.Filter;
				var filter = builds.Eq(_ => _.AccountId, accountId) & builds.Eq(_ => _.InstagramId, instagramAccountId);
				var results = await _ctx.FindAsync(filter);
				return results.FirstOrDefault();
			}
			catch
			{
				return null;
			}
		}
		public async Task<ProxyModel> GetProxyAssigned(string instagramAccountId)
		{
			try
			{
				var builds = Builders<ProxyModel>.Filter;
				var filter = builds.Eq(_ => _.InstagramId, instagramAccountId);
				var results = await _ctx.FindAsync(filter);
				return results.FirstOrDefault();
			}
			catch
			{
				return null;
			}
		}
		public async Task<List<ProxyModel>> GetAllProxyAssigned(string accountId)
		{
			try
			{
				var builds = Builders<ProxyModel>.Filter;
				var filter = builds.Eq(_ => _.AccountId, accountId);
				var results = await _ctx.FindAsync(filter);
				return results.ToList();
			}
			catch
			{
				return new List<ProxyModel>();
			}
		}
		public async Task<List<ProxyModel>> GetAllProxyAssigned()
		{
			try
			{
				var results = await _ctx.FindAsync(_ => true);
				return results.ToList();
			}
			catch
			{
				return new List<ProxyModel>();
			}
		}
		public async Task<List<ProxyModel>> GetAllProxyAssigned(ProxyType type)
		{
			try
			{
				var results = await _ctx.FindAsync(_ => _.ProxyType.Equals(type));
				return results.ToList();
			}
			catch
			{
				return new List<ProxyModel>();
			}
		}
	}

}