using System.Collections.Generic;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessLogic.Handlers.ClientProvider;

namespace Quarkless.HeartBeater.Interfaces.Models
{
	public struct RequestAccountModel
	{
		public ShortInstagramAccountModel InstagramAccount { get; set; }
		public ProfileModel Profile { get; set; }
		public ProxyModel ProxyUsing { get; set; }
	}
	public struct TopicAss
	{
		public string Searchable { get; set; }
		public string FriendlyName { get; set; }
	}
	public struct PopulateAssignments
	{
		public List<TopicAss> TopicsAssigned { get; set; }
		public IAPIClientContainer Worker { get; set; }
	}

	public struct Assignments
	{
		public IAPIClientContainer Worker { get; set; }
		public Topics Topic { get; set; }
		public RequestAccountModel InstagramRequests { get; set; }
	}
}
