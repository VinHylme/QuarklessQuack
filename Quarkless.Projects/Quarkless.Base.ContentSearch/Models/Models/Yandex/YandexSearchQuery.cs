using Quarkless.Base.ContentSearch.Models.Enums;
using Quarkless.Base.ContentSearch.Models.Struct;
using Quarkless.Models.Common.Models.Topic;

namespace Quarkless.Base.ContentSearch.Models.Models.Yandex
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

		public string ColorForYandex
		{
			get
			{
				return Color switch
				{
					ColorType.Any => "any",
					ColorType.ColoredImagesOnly => "color",
					ColorType.BlackAndWhite => "gray",
					ColorType.Red => "red",
					ColorType.Yellow => "yellow",
					ColorType.Orange => "orange",
					ColorType.Cyan => "cyan",
					ColorType.Teal => "cyan",
					ColorType.Green => "green",
					ColorType.Blue => "blue",
					ColorType.Violet => "violet",
					ColorType.Pink => "violet",
					ColorType.Purple => "violet",
					ColorType.White => "white",
					ColorType.Black => "black",
					ColorType.Brown => "black",
					_ => "any"
				};
			}
		}

		public string CorrectSizeTypeFormat
		{
			get
			{
				return Size switch
				{
					SizeType.Large => "large",
					SizeType.Medium => "medium",
					SizeType.Small => "small",
					SizeType.None => "large",
					_ => "large"
				};
			}
		}
	}
}