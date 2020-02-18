using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Base.InstagramUser.Models
{
	public class ChangeBiographyRequest : IExec
	{
		public string Biography { get; set; }
	}
}