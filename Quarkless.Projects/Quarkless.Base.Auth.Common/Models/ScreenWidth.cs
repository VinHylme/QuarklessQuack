using Newtonsoft.Json;

namespace Quarkless.Base.Auth.Common.Models
{
	public class ScreenWidth
	{
		[JsonProperty("width")]
		public int Width { get; set; }

		[JsonProperty("height")]
		public int Height { get; set; }
	}
}