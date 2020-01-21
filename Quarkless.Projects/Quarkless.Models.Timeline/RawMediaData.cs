using Quarkless.Models.Common.Enums;

namespace Quarkless.Models.Timeline
{
	public class RawMediaData
	{
		public string UrlToSend { get; set; }
		public string Media64BaseData { get; set; }
		public MediaSelectionType MediaType { get; set; }
	}
}