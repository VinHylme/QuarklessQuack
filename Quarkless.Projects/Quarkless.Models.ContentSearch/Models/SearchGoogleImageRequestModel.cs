using Quarkless.Models.ContentSearch.Enums;

namespace Quarkless.Models.ContentSearch.Models
{
	public class SearchGoogleImageRequestModel
	{
		public string Prefix { get; set; }
		public string Keyword { get; set; }
		public string Suffix { get; set; }
		public int Limit { get; set; }
		public ColorType Color { get; set; }
		public ImageType MediaType { get; set; }
		public SizeType Size { get; set; }

		public string ColorForGoogle
		{
			get
			{
				return Color switch
				{
					ColorType.Any => "any",
					ColorType.ColoredImagesOnly => "any",
					ColorType.BlackAndWhite => "gray",
					ColorType.Red => "red",
					ColorType.Yellow => "yellow",
					ColorType.Orange => "orange",
					ColorType.Cyan => "teal",
					ColorType.Teal => "teal",
					ColorType.Green => "green",
					ColorType.Blue => "blue",
					ColorType.Violet => "purple",
					ColorType.Pink => "pink",
					ColorType.Purple => "purple",
					ColorType.White => "white",
					ColorType.Black => "black",
					ColorType.Brown => "brown",
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
					SizeType.Large => "l",
					SizeType.Medium => "m",
					SizeType.Small => "i",
					SizeType.None => "l",
					_ => "l"
				};
			}
		}
	}
}