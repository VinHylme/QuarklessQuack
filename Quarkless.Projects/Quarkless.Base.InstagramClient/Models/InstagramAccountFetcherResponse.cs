using InstagramApiSharp.Classes;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.InstagramClient.Models
{
	public class InstagramAccountFetcherResponse
	{
		public ErrorResponse Errors { get; set; }
		public ContextContainer Container { get; set; }
		public IResult<IInstaClient> Response { get; set; }
		public bool SuccessfullyRetrieved { get; set; }
	}
}