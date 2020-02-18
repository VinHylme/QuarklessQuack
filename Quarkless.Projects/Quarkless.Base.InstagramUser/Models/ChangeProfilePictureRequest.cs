using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Base.InstagramUser.Models
{
	public class ChangeProfilePictureRequest : IExec
	{
		public string FileName { get; set; }
	}
}