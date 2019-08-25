using Bogus.DataSets;

namespace QuarklessContexts.Models.FakerModels
{
	public class FakerModel
	{
		public Name.Gender Gender { get; set; }
		public string Email { get; set; }
		public string Username { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set;  }
		public string Password { get; set; }
		public string UserAgent { get; set; }
	}
}
