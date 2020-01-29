using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Models.Media
{
	public class DeleteMediaModel : IExec
	{
		public string MediaId { get; set; }
		public int MediaType { get; set; }
	}
}