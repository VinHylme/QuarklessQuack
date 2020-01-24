using Bogus.DataSets;
using Quarkless.Utilities.Models.ProxyModels;

namespace Quarkless.Utilities.Models.Person
{
	public class PersonCreateModel
	{
		public string Topic { get; set; }
		public Name.Gender Gender { get; set; }
		public string[] PossibleEmails { get; set; }
		public string[] PossibleUsernames { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Password { get; set; }
		public string UserAgent { get; set; }
		public string PhoneNumber { get; set; }
		public Proxy Proxy { get; set; }
	}
}
