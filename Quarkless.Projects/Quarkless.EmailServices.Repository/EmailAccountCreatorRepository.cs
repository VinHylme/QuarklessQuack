using Quarkless.Repository.MongoContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Quarkless.EmailServices.Models;
using Quarkless.SmsHandler.Models.Enums;

namespace Quarkless.EmailServices.Repository
{
	public class EmailAccountCreatorRepository : IEmailAccountCreatorRepository
	{
		private readonly IMongoCollection<EmailAccount> _ctx;
		private const string COLLECTION_NAME = "EmailAccounts";
		public EmailAccountCreatorRepository(IMongoClientContext context)
		{
			_ctx = context.CreatorDatabase.GetCollection<EmailAccount>(COLLECTION_NAME);
		}
		public async Task AddAccount(EmailAccount account)
		{
			try
			{
				await _ctx.InsertOneAsync(account);
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
			}
		}
		public async Task<bool> UpdateExistingService(string emailId, string serviceId, UsedBy updated)
		{
			try
			{
				var filter = Builders<EmailAccount>.Filter.Where(
					x => x._id == emailId && x.UsedBy.Any(_ => _._id == serviceId));

				var update = Builders<EmailAccount>.Update.Set(x => x.UsedBy[-1], updated);
				var res = await _ctx.FindOneAndUpdateAsync(filter, update);
				return true;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return false;
			}
		}
		public async Task<string> AddService(string id, UsedBy service)
		{
			try
			{
				var filter = Builders<EmailAccount>.Filter.Eq("_id", new ObjectId(id));
				service._id = ObjectId.GenerateNewId(Environment.TickCount).ToString();
				var update = Builders<EmailAccount>.Update.Push(_ => _.UsedBy, service);
				await _ctx.FindOneAndUpdateAsync(filter, update);
				return service._id;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return string.Empty;
			}
		}
		public async Task<bool> RemoveService(string emailId, string serviceId)
		{
			try
			{
				var filter = Builders<EmailAccount>.Filter.Eq("_id", new ObjectId(emailId));
				var update = Builders<EmailAccount>.Update.PullFilter(_ => _.UsedBy, _ => _._id == serviceId);
				await _ctx.FindOneAndUpdateAsync(filter, update);
				return true;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return false;
			}
		}
		public async Task<List<EmailAccount>> GetAllEmailAccounts()
		{
			return (await _ctx.FindAsync(_ => true)).ToList();
		}
		public async Task<List<EmailAccount>> GetUnusedAccounts(ByService notUsedBy)
		{
			try
			{
				var builders = Builders<EmailAccount>.Filter;
				var filter = builders.Eq("UsedBy", new List<UsedBy>())
							 | builders.ElemMatch(_ => _.UsedBy, a => a.By != (int)notUsedBy && !a.HasFailed);

				var results = await _ctx.FindAsync(filter);
				return results.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
			}
			return new List<EmailAccount>();
		}
		public async Task<EmailAccount> GetGmailAccount(string id)
		{
			try
			{
				var builders = Builders<EmailAccount>.Filter;
				var filter = builders.Eq("_id", new ObjectId(id));
				var results = await _ctx.FindAsync(filter);
				return results.First();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
	}
}
