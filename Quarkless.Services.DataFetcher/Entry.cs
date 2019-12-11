using System;

namespace Quarkless.Services.DataFetcher
{
	/// <summary>
	/// Purpose of this project is to extract and store data for each type of topic category available for instagram
	/// </summary>
	internal class Entry
	{
		//TODO: Need to get a list of all current available topics for instagram
		//TODO: All should be extracted by specific type of worker accounts (not currently assigned to customer accounts)
		//TODO: Create fetch function for Posts Captions, Hashtags and comments (possibly account details)
		//TODO: If fetching instagram posts you have access to the captions, preview comments and possibly hashtags
		//TODO: Hashtags are also available on the first comment of a post too
		//TODO: Make sure data is clean and not duplicated
		//TODO: Captions and Comments extracted should not have any @mentions, #hashtags, advertising or non word symbols (expect emojis)
		//TODO: Store in database
		//TODO: These should be generic for that particular category,
		//TODO: If customer wants to create a clothing topic it should be generic to that topic
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
		}
	}
}
