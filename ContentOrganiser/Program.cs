using System;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ContentOrganiser
{
	class Program
	{
		static void Main(string[] args)
		{
			RequestHandler.RequestHandler requestHandler = new RequestHandler.RequestHandler();
			var na = requestHandler.GetRequest("food");
			string pattern = @"(http|https|ftp)://([\w|.|/|-]+)?(jpg|jpeg)";
			Regex regex = new Regex(pattern);
			var urls = regex.Matches(na);
			var path = AppContext.BaseDirectory;



			foreach (var url in urls)
				requestHandler.DownloadImage(url.ToString(), $@"{path}\Images");
			Console.Read();
		}
		//public void old() {
		//	{
		//		var image = requestHandler.GetImageFromUrl(urls[150].ToString());

		//		Bitmap bitmap = ResizeImage(image, 250, 250);
		//		for (int x = 0; x < bitmap.Width; x++)
		//		{
		//			for (int y = 0; y < bitmap.Height; y++)
		//			{
		//				var curr = bitmap.GetPixel(x, y);
		//				r += curr.R;
		//				g += curr.G;
		//				b += curr.B;
		//				total++;
		//				//colorsFromImage.Add();
		//			}
		//		}
		//	}
		//	Console.WriteLine($"R: {r}G: {g }B: {b}");
		//	Console.WriteLine($"R: {r / total}G: {g / total}B: {b / total}");
		//}
		public static Bitmap ResizeImage(Image image, int width, int height)
		{
			var destRect = new Rectangle(0, 0, width, height);
			var destImage = new Bitmap(width, height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (var wrapMode = new ImageAttributes())
				{
					wrapMode.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}

			return destImage;
		}
	}
}
