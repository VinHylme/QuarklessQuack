using InstagramApiSharp.Classes;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.InstagramClient.Interfaces;

namespace Quarkless.Models.InstagramClient
{
	public class InstagramAccountFetcherResponse
	{
		public ErrorResponse Errors { get; set; }
		public ContextContainer Container { get; set; }
		public IResult<IInstaClient> Response { get; set; }
		public bool SuccessfullyRetrieved { get; set; }
	}
}