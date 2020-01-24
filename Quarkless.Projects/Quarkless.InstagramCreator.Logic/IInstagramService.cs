using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.EmailServices.Models;
using Quarkless.InstagramCreator.Models;
using Quarkless.InstagramCreator.Models.Enums;
using Quarkless.Utilities.Models.Person;

namespace Quarkless.InstagramCreator.Logic
{
	
	public interface IInstagramService
	{
		/// <summary>
		/// should only be used for email method
		/// </summary>
		/// <param name="emailAccount"></param>
		/// <returns></returns>
		Task<InstagramAccountCreationEnum> StartService(EmailAccount emailAccount);
		/// <summary>
		/// should only be used for phone method
		/// </summary>
		/// <param name="person"></param>
		/// <returns></returns>
		Task<InstagramAccountCreationEnum> StartService(PersonCreateModel person);

		Task<InstagramAccountCreationResponse> CreateInstagramAccount(EmailAccount emailAccount);
		Task<List<InstagramAccount>> GetInstagramAccounts();
		Task StartTest(InstagramAccount person);
	}
}