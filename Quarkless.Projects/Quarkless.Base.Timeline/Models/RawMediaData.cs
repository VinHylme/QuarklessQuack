using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Timeline.Models
{
	public class RawMediaData
	{
		public string UrlToSend { get; set; }
		public string Media64BaseData { get; set; }
		public MediaSelectionType MediaType { get; set; }
	}
}