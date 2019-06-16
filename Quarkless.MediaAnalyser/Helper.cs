using FFmpeg.NET;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using Shipwreck.Phash.Data;
namespace Quarkless.MediaAnalyser
{
	public static class  Helper
	{
		private static string GetFilePathByName(string name)
		{
			var settingPath = Path.GetFullPath(Path.Combine(@"..\Quarkless"));
			IConfiguration configuration = new ConfigurationBuilder().
				SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();
			return configuration["MediaPath:" + name];
		}
		public static IEnumerable<byte[]> RemoveDuplicates (this IEnumerable<byte[]> images, byte[] image, double scorePercantage)
		{
			var orginalHash = ImagePhash.ComputeDigest(image.ByteToBitmap().ToLuminanceImage());
			foreach (var currImage in images)
			{
				var bitmap = (Bitmap)currImage.ByteToBitmap();
				var Currenthash = ImagePhash.ComputeDigest(bitmap.ToLuminanceImage());
				var score = ImagePhash.GetCrossCorrelation(Currenthash, orginalHash);
				if (score < scorePercantage)
				{
					yield return currImage;
				}
				else
				{
					continue;
				}
			}
		}
		public static bool DoesExist(this byte[] image, IEnumerable<byte[]> images, double scorePercantage)
		{
			var orginalHash = ImagePhash.ComputeDigest(image.ByteToBitmap().ToLuminanceImage());
			foreach(var currImage in images)
			{
				var bitmap = (Bitmap)currImage.ByteToBitmap();
				var Currenthash = ImagePhash.ComputeDigest(bitmap.ToLuminanceImage());
				var score = ImagePhash.GetCrossCorrelation(Currenthash, orginalHash);
				if (score > scorePercantage)
				{
					return true;
				}
				else
				{
					continue;
				}
			}
			return false;
		}
		public static byte[] DownloadMedias(this List<string> urls, int poz)
		{
			using (WebClient webClient = new WebClient())
			{
				try
				{
					if (poz < 0) { return null; }
					return webClient.DownloadData(urls.ElementAtOrDefault(poz));
				}
				catch (Exception e)
				{
					DownloadMedias(urls, poz--);
					return null;
				}
			}
		}
		public static byte[] DownloadMedia(this string url)
		{
			using (WebClient webClient = new WebClient())
			{
				try
				{
					return webClient.DownloadData(url);
				}
				catch (Exception e)
				{
					return null;
				}
			}
		}
		public static Bitmap ByteToBitmap(this byte[] imagesByte)
		{
			using (var ms = new MemoryStream(imagesByte))
			{
				return new Bitmap(ms);
			}
		}
		public static byte[] BitmapToByte(this Bitmap bitmap)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				bitmap.Save(ms, bitmap.RawFormat);
				return ms.ToArray();
			}
		}
		public static Color GetDominantColor(this Bitmap bmp)
		{
			if (bmp == null)
			{
				throw new ArgumentNullException("bmp");
			}

			BitmapData srcData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

			int bytesPerPixel = Image.GetPixelFormatSize(srcData.PixelFormat) / 8;

			int stride = srcData.Stride;

			IntPtr scan0 = srcData.Scan0;

			long[] totals = new long[] { 0, 0, 0 };

			int width = bmp.Width * bytesPerPixel;
			int height = bmp.Height;

			unsafe
			{
				byte* p = (byte*)(void*)scan0;

				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x += bytesPerPixel)
					{
						totals[0] += p[x + 0];
						totals[1] += p[x + 1];
						totals[2] += p[x + 2];
					}

					p += stride;
				}
			}

			long pixelCount = bmp.Width * height;

			int avgB = Convert.ToInt32(totals[0] / pixelCount);
			int avgG = Convert.ToInt32(totals[1] / pixelCount);
			int avgR = Convert.ToInt32(totals[2] / pixelCount);

			bmp.UnlockBits(srcData);

			return System.Drawing.Color.FromArgb(avgR, avgG, avgB);
		}
		public static byte[] MostSimilarImage(this Color color, List<byte[]> images)
		{
			List<Color> dom_colors = new List<Color>();
			foreach (var image in images)
			{
				var bitmap = image.ByteToBitmap().ReduceBitmap(250, 250);
				dom_colors.Add(GetDominantColor(bitmap));
			}

			var colorDiffs = dom_colors.Select(n => ColorDiff(n, color)).Min(n => n);
			var pos = dom_colors.FindIndex(n => ColorDiff(n, color) == colorDiffs);
			var sel = images.ElementAtOrDefault(pos);
			return sel;
		}
		private static int ColorDiff(Color c1, Color c2)
		{
			return (int)Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
								   + (c1.G - c2.G) * (c1.G - c2.G)
								   + (c1.B - c2.B) * (c1.B - c2.B));
		}
		public static Bitmap ReduceBitmap(this Bitmap original, int reducedWidth, int reducedHeight)
		{
			var reduced = new Bitmap(reducedWidth, reducedHeight);
			using (var dc = Graphics.FromImage(reduced))
			{
				dc.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				dc.DrawImage(original, new Rectangle(0, 0, reducedWidth, reducedHeight), new Rectangle(0, 0, original.Width, original.Height), GraphicsUnit.Pixel);
			}

			return reduced;
		}
		public static IEnumerable<Bitmap> ReadImagesFromDirectory(string path, string pattern)
		{
			var images_ = Directory.EnumerateFiles(path, pattern);
			foreach (var image_ in images_)
			{
				var fil = File.ReadAllBytes(image_);
				var meme = new MemoryStream(fil);
				if (meme != null)
					yield return new Bitmap(Bitmap.FromStream(meme));
			}
		}
		public static byte[] GenerateVideoThumbnail(this byte[] video, int specificFrame = 5)
		{
			string path = GetFilePathByName("videosTempPath");
			string outp = GetFilePathByName("imagesTempPath");
			string engine_path = GetFilePathByName("enginePath");
			string videoPath = string.Format(path + "video_{0}_{1}.mp4", Guid.NewGuid(), DateTime.UtcNow.ToLongDateString());
			File.WriteAllBytes(videoPath, video);
			try
			{
				MediaFile mediaFile = new MediaFile(videoPath);
				var engine = new Engine(engine_path);
				var meta = engine.GetMetaDataAsync(mediaFile).GetAwaiter().GetResult();
				var opt = new ConversionOptions { Seek = TimeSpan.FromSeconds(specificFrame) };
				var outputFile = new MediaFile((string.Format(@"{0}image-{1}_{2}.jpeg", outp, Guid.NewGuid(), DateTime.UtcNow.ToLongDateString())));
				engine.GetThumbnailAsync(mediaFile, outputFile, opt);

				var image = File.ReadAllBytes(outputFile.FileInfo.FullName);

				File.Delete(videoPath);
				File.Delete(outputFile.FileInfo.FullName);

				return image;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public static byte[] MostSimilarVideo(this Color profileColor, List<byte[]> videos, int frameSkip = 5)
		{
			string path = GetFilePathByName("videosTempPath");
			string outp = GetFilePathByName("imagesTempPath");
			string engine_path = GetFilePathByName("enginePath");
			string videoPath = string.Format(path + "video_{0}_{1}.mp4", Guid.NewGuid(), DateTime.UtcNow.ToLongDateString());
			List<Color> dom_colors = new List<Color>();

			foreach (var video in videos)
			{
				File.WriteAllBytes(videoPath, video);
				try
				{
					MediaFile mediaFile = new MediaFile(videoPath);
					var engine = new Engine(engine_path);
					var meta = engine.GetMetaDataAsync(mediaFile).GetAwaiter().GetResult();
					var i = 0;
					while (i < meta.Duration.Seconds)
					{
						var opt = new ConversionOptions { Seek = TimeSpan.FromSeconds(frameSkip) };
						var outputFile = new MediaFile((string.Format(@"{0}image-{1}_{2}_{3}.jpeg", outp, i, Guid.NewGuid(), DateTime.UtcNow.ToLongDateString())));
						engine.GetThumbnailAsync(mediaFile, outputFile, opt);
						i++;
					}
					List<Bitmap> videoframes = ReadImagesFromDirectory(outp, "*.jpeg").ToList();
					List<Color> video_colors_avg = new List<Color>();
					if (videoframes != null)
					{
						videoframes.ForEach(im => video_colors_avg.Add(GetDominantColor(im)));
						int r = 0, b = 0, g = 0;
						video_colors_avg.ForEach(c => {
							r += c.R;
							b += c.B;
							g += c.G;
						});
						int total = video_colors_avg.Count;
						dom_colors.Add(Color.FromArgb(r / total, b / total, g / total));
					}
				}
				catch (Exception ee)
				{
					Console.WriteLine(ee.Message);
					return null;
				}
			}
			var colorDiffs = dom_colors.Select(n => ColorDiff(n, profileColor)).Min(n => n);
			var pos = dom_colors.FindIndex(n => ColorDiff(n, profileColor) == colorDiffs);
			Directory.EnumerateFiles(outp, "*.jpeg").ToList().ForEach(f => File.Delete(f));
			Directory.EnumerateFiles(path, "*.mp4").ToList().ForEach(f => File.Delete(f));
			return videos.ElementAtOrDefault(pos);
		}
	}
}
