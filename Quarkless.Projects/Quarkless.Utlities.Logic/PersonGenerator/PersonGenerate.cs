using Newtonsoft.Json;
using Quarkless.Utilities.Models.Muse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Quarkless.Utilities.Models.Enums;
using Quarkless.Utilities.Models.Person;
using Quarkless.Utilities.Models.Extensions;
using Bogus.DataSets;

namespace Quarkless.Utlities.Logic.PersonGenerator
{
	public class PersonGenerate
	{
		private const string DRY_CODES_URL = "http://names.drycodes.com/";

		private const string DATA_MUSE_URL_BASE = "https://api.datamuse.com";
		private const string DATA_MUSE_URL_API = DATA_MUSE_URL_BASE + "/words?";
		private const string DATA_MUSE_SIMILAR_WORDS = "ml=0";
		private const string DATA_MUSE_RELATED_STARTS_WITH = DATA_MUSE_SIMILAR_WORDS+"&sp=1*";
		private const string DATA_MUSE_RELATED_ENDS_WITH = DATA_MUSE_SIMILAR_WORDS+"&sp=*1";
		private const string DATA_MUSE_WORDS_THAT_SOUNDS_LIKE = "sl=0";
		private const string DATA_MUSE_WORDS_THAT_RHYME = "rel_rhy=0";
		private const string DATA_MUSE_WORDS_THAT_RHYME_AND_RELATE_TO = DATA_MUSE_SIMILAR_WORDS + "&rel_rhy=1";
		private const string DATA_MUSE_ADJECTIVES_DESCRIBE = "rel_jjb=0";
		private const string DATA_MUSE_WITH_TOPIC = "&topics=0";
		private const string DATA_MUSE_NOUNS_USED_TO_DESCRIBE_WORD = "rel_jja=0";
		private const string DATA_MUSE_STRONGLY_ASSOCIATED_WITH = "rel_trg=0";
		private const string DATA_MUSE_SUGGEST = "/sug?s=0";
		private const int USERNAME_ATTEMPTS = 5;
		private readonly Random _random;
		public PersonGenerate()
		{
			_random = new Random(Environment.TickCount);
		}
		
		public async Task<string[]> GenerateUsername(string topic, int takeAmount = 1, int maxWordLength = 5, string separateBy = ".")
		{
			if(takeAmount <= 0 || takeAmount > 3)
				throw new Exception("Take amount can only be between 1 and 3");

			var associatedUrl = DATA_MUSE_URL_API + DATA_MUSE_STRONGLY_ASSOCIATED_WITH.Format(topic);

			var usernames = new string[USERNAME_ATTEMPTS];
			const int attempts = 3;
			var currentAttempt = 0;

			using (var client = new HttpClient())
			{
				var associatedResponse = JsonConvert.DeserializeObject<IEnumerable<WordResponse>>
					(await client.GetStringAsync(associatedUrl));

				if (!associatedResponse.Any())
					return new string[]{};

				string[] pickRhyme = null;
				string[] pickNoun = null;

				Retry:
				if (currentAttempt > attempts)
					return new string[] { };

				var pick = associatedResponse.Where(_=>_.Word.Length < maxWordLength).TakeAny(USERNAME_ATTEMPTS, _random)
					.Select(_ => _.Word.Replace(" ", "")).ToArray();

				if (!pick.Any())
					return await GenerateUsername(topic, takeAmount, maxWordLength);

				if (takeAmount > 1)
				{	
					var rhymeWordsUrl = DATA_MUSE_URL_API + DATA_MUSE_WORDS_THAT_RHYME.Format(pick[0]);
					var rhymeWordsResponse = JsonConvert.DeserializeObject<IEnumerable<WordResponse>>
						(await client.GetStringAsync(rhymeWordsUrl));

					if (!rhymeWordsResponse.Any())
					{
						currentAttempt++;
						goto Retry;
					}

					pickRhyme = rhymeWordsResponse.Where(_ => _.Word.Length < maxWordLength)
						.TakeAny(USERNAME_ATTEMPTS, _random)
						.Select(_ => _.Word.Replace(" ", "")).ToArray();
					
					if (takeAmount > 2)
					{
						var randomColor = Enum.GetValues(typeof(Colors)).Cast<Colors>()
							.TakeAny(1, _random).First().ToString();

						var nounsDescribeUrl =
							DATA_MUSE_URL_API + DATA_MUSE_NOUNS_USED_TO_DESCRIBE_WORD.Format(randomColor);

						var nounsDescribeResponse = JsonConvert.DeserializeObject<IEnumerable<WordResponse>>
							(await client.GetStringAsync(nounsDescribeUrl));

						if (!nounsDescribeResponse.Any())
						{
							currentAttempt++;
							goto Retry;
						}

						pickNoun = nounsDescribeResponse
							.Where(_ => _.Word.Length < maxWordLength)
							.TakeAny(USERNAME_ATTEMPTS, _random)
							.Select(_ => _.Word.Replace(" ", "")).ToArray();
					}
				}

				var pos = 0;

				do
				{
					var final = pos >= pick.Length ? pick.TakeAny(1, _random).First() : pick[pos];

					if (pickRhyme!=null && pickRhyme.Any())
					{
						if (pos >= pickRhyme.Length)
							final += $"{separateBy}{pickRhyme.TakeAny(1, _random).First()}";
						else
							final += $"{separateBy}{pickRhyme[pos]}";
						
					}

					if (pickNoun!=null && pickNoun.Any())
					{
						if (pos >= pickNoun.Length)
							final += $"{separateBy}{pickNoun.TakeAny(1, _random).First()}";
						else
							final += $"{separateBy}{pickNoun[pos]}";
					}

					usernames[pos] = final;

					pos++;
				} while (pos < USERNAME_ATTEMPTS);
			}

			return usernames;
		}

		public async Task<PersonCreateModel> GeneratePerson(string topic, string locale = "en",
			string emailProvider = "gmail.com", bool? isMale = null)
		{
			if (string.IsNullOrEmpty(emailProvider))
				emailProvider = "gmail.com";

			var faker = new Bogus.Faker(locale);
			const string separateBy = ".";
			const int maxAttempts = 3;
			var currentAttempt = 0;
			Name.Gender gender;
			if (isMale != null)
				gender = isMale == true ? Name.Gender.Male : Name.Gender.Female;
			else
				gender = faker.Person.Gender;

			var firstName = faker.Name.FirstName(gender);
			var lastName = faker.Name.LastName(gender);

			if (string.IsNullOrEmpty(lastName)) await GeneratePerson(topic, locale, emailProvider, isMale);
			var password = CreatePassword(firstName);
			var userAgent = faker.Internet.UserAgent();
			Retry:
			if (currentAttempt > maxAttempts)
				return null;
			var usernames = await GenerateUsername(topic, 2, 13, separateBy:separateBy);
			if (!usernames.Any())
			{
				currentAttempt++;
				goto Retry;
			}
			var emails = new string[USERNAME_ATTEMPTS];
			for (var x = 0; x < emails.Length; x++)
			{
				emails[x] =
					$"{firstName}.{usernames[x].Substring(0, usernames[x].IndexOf(separateBy, StringComparison.CurrentCulture))}{GenerateRandomSequences()}@{emailProvider}";
			}
			return new PersonCreateModel
			{
				Topic = topic,
				PossibleUsernames = usernames,
				PossibleEmails = emails,
				UserAgent = userAgent,
				FirstName = firstName,
				LastName = lastName,
				Gender = gender,
				Password = password,
			};
		}

		public int GenerateRandomSequences(int length = 3)
		{
			var sequence = new int[length];
			for (var x = 0; x < length; x++)
			{
				sequence[x] = _random.Next(0, 9);
			}

			var str = string.Join("",Array.ConvertAll(sequence, item => item.ToString()));

			return int.Parse(str);
		}
		public string CreatePassword(string prefix = null, string suffix = null, int length = 12)
		{
			const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.$!?";
			var res = new StringBuilder();
			while (0 < length--)
			{
				res.Append(valid[_random.Next(valid.Length)]);
			}

			var password = res.ToString();

			if (!string.IsNullOrEmpty(prefix))
				password = prefix + password;
			if (!string.IsNullOrEmpty(suffix))
				password += suffix;

			return password;
		}

	}
}
