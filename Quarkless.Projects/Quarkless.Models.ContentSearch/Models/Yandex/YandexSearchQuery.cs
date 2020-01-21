using Quarkless.Models.ContentSearch.Enums;
using Quarkless.Models.ContentSearch.Struct;
using Quarkless.Models.Topic;

namespace Quarkless.Models.ContentSearch.Models.Yandex
{
	public class YandexSearchQuery
	{
		public CTopic OriginalTopic { get; set; }
		public string SearchQuery { get; set; }
		public Orientation Orientation { get; set; }
		public ImageType Type { get; set; }
		public ColorType Color { get; set; }
		public FormatType Format { get; set; }
		public SizeType Size { get; set; } = SizeType.None;
		public SpecificSize? SpecificSize { get; set; } = null;
	}
}