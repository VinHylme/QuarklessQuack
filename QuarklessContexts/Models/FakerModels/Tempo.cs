using Bogus.DataSets;
using InstagramApiSharp.Classes;

namespace QuarklessContexts.Models.FakerModels
{
	public class Tempo
	{
		public Name.Gender Gender;
		public string FirstName;
		public string LastName;
		public string Username;
		public string Password;
		public string UserAgent;
		public string Email;
		public IResult<InstaAccountCreation> InResult;

	}
}
