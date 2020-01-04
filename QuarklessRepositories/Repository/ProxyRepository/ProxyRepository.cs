using MongoDB.Bson;
using System.Collections.Generic;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using QuarklessRepositories.RepositoryClientManager;
using QuarklessContexts.Models.Proxies;

namespace QuarklessRepositories.ProxyRepository
{
	public class ProxyRepository : IProxyRepository
	{
		private readonly IRepositoryContext _context;

		public ProxyRepository(IRepositoryContext context)
		{
			_context = context;
		}

		public async void AddProxies(List<ProxyModel> proxies)
		{
			await _context.Proxies.InsertManyAsync(proxies);
		}

		public async void AddProxy(ProxyModel proxy)
		{
			await _context.Proxies.InsertOneAsync(proxy);
		}

		public async Task<bool> AssignProxy(AssignedTo assignedTo)
		{
			assignedTo.AssignedDate = DateTime.UtcNow;
			var builders = Builders<ProxyModel>.Filter;
			var filters = builders.Eq("AssignedTo.Account_Id",BsonNull.Value.AsNullableObjectId) & builders.Eq("AssignedTo.InstaId",BsonNull.Value.AsNullableObjectId);
			var update = Builders<ProxyModel>.Update.Set(_=>_.AssignedTo, assignedTo);
			try
			{
				var proxy = await _context.Proxies.UpdateOneAsync(filters, update);

				if (proxy.IsAcknowledged)
				{
					return true;
				}
				return false;
			}
			catch (Exception ee)
			{
				return false;
			}
		}

		public async Task<IEnumerable<ProxyModel>> GetAllAssignedProxies()
		{
			//var filter = Builders<ProxyModel>.Filter.Ne("AssignedTo.InstaId", BsonNull.Value.AsNullableObjectId);
			var results = await _context.Proxies.FindAsync(_=>true);
			if (results != null)
			{
				return results.ToList();
			}
			return null;
		}

		public async Task<IEnumerable<ProxyModel>> GetAssignedProxyByAccountId(string accountId)
		{
			var filter = Builders<ProxyModel>.Filter.Eq("AssignedTo.Account_Id", accountId);
			var results = await _context.Proxies.FindAsync(filter);
			if (results != null)
			{
				return results.ToList();
			}
			return null;
		}
		public async Task<ProxyModel> GetAssignedProxyOf(string accountId, string instagramAccountId)
		{
			var builders = Builders<ProxyModel>.Filter;
			var filter = builders.Eq("AssignedTo.Account_Id",accountId) & builders.Eq("AssignedTo.InstaId", instagramAccountId);
			var results = await _context.Proxies.FindAsync(filter);
			return results?.FirstOrDefault();
		}

		public async Task<ProxyModel> GetAssignedProxyByInstaId(string instagramAccountId)
		{
			var filter = Builders<ProxyModel>.Filter.Eq("AssignedTo.InstaId",instagramAccountId);
			var results = await _context.Proxies.FindAsync(filter);
			return results?.FirstOrDefault();
		}

		public async Task<bool> RemoveUserFromProxy(AssignedTo assignedTo)
		{
			var builders = Builders<ProxyModel>.Filter;
			var findfilters = builders.Eq("AssignedTo.Account_Id", assignedTo.Account_Id) & builders.Eq("AssignedTo.InstaId", assignedTo.InstaId);

			var currentFindings = await _context.Proxies.FindAsync(findfilters,new FindOptions<ProxyModel, ProxyModel>() 
			{ 
				Sort = Builders<ProxyModel>.Sort.Ascending(_=>_.AssignedTo.AssignedDate)
			});
			var updatefilters = builders.Eq("AssignedTo.Account_Id", assignedTo.Account_Id) & builders.Eq("AssignedTo.InstaId", assignedTo.InstaId) & builders.Eq("AssignedTo.AssignedDate",currentFindings.FirstOrDefault().AssignedTo.AssignedDate);
			var update = Builders<ProxyModel>.Update.Set(_ => _.AssignedTo, new AssignedTo { });
			try
			{
				var proxy = await _context.Proxies.UpdateOneAsync(updatefilters, update);

				return proxy.IsAcknowledged;
			}
			catch
			{
				return false;
			}
		}
	}
}
