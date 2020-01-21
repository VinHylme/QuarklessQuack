using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quarkless.Models.TranslateService
{
	public class DetectLanguageResponse
	{
		[JsonProperty("detection")]
		public List<string> Detections { get; set; }
	}
}