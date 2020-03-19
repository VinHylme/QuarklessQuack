using Bogus;
using Bogus.DataSets;
using Quarkless.Base.ContentSearch;
using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.EmailService.Interfaces;
using Quarkless.Models.HashtagGenerator.Interfaces;
using Quarkless.Models.TextGenerator.Interfaces;
using Quarkless.Models.TranslateService.Interfaces;
using Quarkless.Models.Utilities;
using Quarkless.Models.Utilities.Interfaces;

namespace Quarkless.Logic.Utilities
{
	public class UtilProviders : IUtilProviders
	{
		public UtilProviders(ITextGenerator textGenerator, IHashtagGenerator hashtagGenerator,
			ISearchProvider searchProvider)
		{
			TextGenerator = textGenerator;
			HashtagGenerator = hashtagGenerator;
			//EmailService = emailService;
			SearchProvider = searchProvider;
		}
		public ISearchProvider SearchProvider { get; }
		public IHashtagGenerator HashtagGenerator { get; }
		public ITextGenerator TextGenerator { get; }
		//public ITranslateService TranslateService { get; }
		//public IEmailService EmailService { get; }

		public FakerModel GeneratePerson(string locale = "en", string emailProvider = null, bool? isMale = null)
		{
			var faker = new Faker(locale);
			Name.Gender gender;
			if (isMale != null)
				gender = isMale == true ? Name.Gender.Male : Name.Gender.Female;
			else
				gender = faker.Person.Gender;

			var firstName = faker.Name.FirstName(gender);
			var lastName = faker.Name.LastName(gender);

			var userName = faker.Internet.UserName(firstName, lastName) + "_" + SecureRandom.Next(1,1050);

			var password = faker.Internet.Password(SecureRandom.Next(13,25), true, prefix: lastName.Substring(0,lastName.Length/2));
			
			var userAgent = faker.Internet.UserAgent();
			string email;
			if(emailProvider!=null)
				email = faker.Internet.Email(firstName, lastName, emailProvider, (SecureRandom.Next(12,22)+faker.UniqueIndex).ToString());
			else
			{
				email = faker.Internet.Email(firstName, lastName,
					(SecureRandom.Next(13, 23) + faker.UniqueIndex).ToString());
			}

			return new FakerModel
			{
				Email = email,
				FirstName = firstName,
				Gender = gender,
				LastName = lastName,
				Password = password,
				Username = userName,
				UserAgent = userAgent
			};
		}
	}
}
