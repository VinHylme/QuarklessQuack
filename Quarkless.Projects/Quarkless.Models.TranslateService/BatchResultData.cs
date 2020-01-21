using System.Collections.Generic;

namespace Quarkless.Models.TranslateService
{
	public class BatchResultData
	{
		public List<List<Detection>> detections { get; set; }
	}
}