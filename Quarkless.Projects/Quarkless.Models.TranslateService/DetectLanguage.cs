using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quarkless.Models.TranslateService
{
	public class DetectLanguage
	{
		[JsonProperty("text")]
		public List<string> Text { get; set; }
	}
}