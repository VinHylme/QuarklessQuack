using System.Collections.Generic;
using Quarkless.Models.Common.Models;

namespace Quarkless.Events.Models.PublishObjects
{
	public class ProfileModel
	{
		public string _id { get; set; }
		public string Account_Id { get; set; }
		public string InstagramAccountId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Language { get; set; }
		//public Topic ProfileTopic { get; set; }
		public bool AutoGenerateTopics { get; set; }
		public List<string> UserTargetList { get; set; }
		public List<Location> LocationTargetList { get; set; }
	}
}
