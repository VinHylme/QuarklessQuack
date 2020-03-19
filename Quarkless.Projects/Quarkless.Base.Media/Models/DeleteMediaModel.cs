using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Base.Media.Models
{
	public class DeleteMediaModel : IExec
	{
		public string MediaId { get; set; }
		public int MediaType { get; set; }
	}
}