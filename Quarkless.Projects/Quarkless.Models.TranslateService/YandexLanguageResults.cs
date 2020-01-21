using Newtonsoft.Json;

namespace Quarkless.Models.TranslateService
{
	public struct YandexLanguageResults
	{
		[JsonProperty("lang")]
		public string Lang { get; set; }
		[JsonProperty("code")]
		public int Code { get; set; }
		[JsonProperty("message")]
		public string Message { get; set; }
	}
}