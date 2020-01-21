namespace Quarkless.Models.TranslateService
{
	public class Detection
	{
		public string language { get; set; }
		public bool isReliable { get; set; }
		public float confidence { get; set; }
	}
}