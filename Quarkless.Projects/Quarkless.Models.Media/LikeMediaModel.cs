using Quarkless.Models.Common.Interfaces;
using Quarkless.Models.Common.Models.Resolver;

namespace Quarkless.Models.Media
{
	public class LikeMediaModel : IExec
	{
		public MediaShort Media { get; set; }
		public UserShort User { get; set; }
		public DataFrom DataFrom { get; set; }
	}
}