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
using System.Drawing.Drawing2D;
using ImageProcessor.Imaging.Filters.EdgeDetection;
using ImageProcessor.Imaging;

namespace Quarkless.MediaAnalyser
{
	public static class  Helper
	{
		public struct ImageHolder
		{
			public byte[] OriginalImageData;
			public byte[] ReducedImageData;
		}
		private static string GetFilePathByName(string name)
		{
			var settingPath = Path.GetFullPath(Path.Combine(@"..\Quarkless"));
			IConfiguration configuration = new ConfigurationBuilder().
				SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();
			return configuration["MediaPath:" + name];
		}
		public static bool ImageIsDuplicate(this byte[] image, byte[] targetImage, double scorePerctange)
		{
			var ogHash = ImagePhash.ComputeDigest(image.ByteToBitmap().ToLuminanceImage());
			var targetHash = ImagePhash.ComputeDigest(targetImage.ByteToBitmap().ToLuminanceImage());
			return ImagePhash.GetCrossCorrelation(ogHash,targetHash) > scorePerctange;
		}
		public static IEnumerable<byte[]> SelectImageOnSize(this IEnumerable<byte[]> imageData, Size imSize)
		{
			List<byte[]> toreturn = new List<byte[]>();
			foreach(var image in imageData)
			{
				if (image != null) {
					try { 
					var bitmapImage = image.ByteToBitmap();
					if (bitmapImage != null) { 
						if(bitmapImage.Width > imSize.Width && bitmapImage.Height > imSize.Height)
							toreturn.Add(image);
						}
					}
					catch(Exception ee)
					{
						Console.WriteLine(ee.Message);
						continue;
					}
				}
			}
			return toreturn;
		}
		public static IEnumerable<byte[]> RemoveDuplicateImages (this IEnumerable<byte[]> selfImages,
			IEnumerable<byte[]> targetImages, double scorePercantage)
		{
			if(selfImages.Count()<=0) throw new Exception("self images cannot be empty");
			if(targetImages.Count()<=0) throw new Exception("target images cannot be empty");
			targetImages = targetImages.SelectImageOnSize(new Size(600,600));
			//initialise
			ImageHolder[] selfImageHolder = new ImageHolder[selfImages.Count()];
			Parallel.For(0, selfImageHolder.Length, pos =>
			{
				selfImageHolder[pos].OriginalImageData = selfImages.ElementAtOrDefault(pos);
				selfImageHolder[pos].ReducedImageData = selfImages.ElementAtOrDefault(pos)
					.Constrain(new Size(135, 135))
					.EntropyCrop(180)
					.DetectEdges(new Laplacian3X3EdgeFilter(), true);
			});

			ImageHolder[] targetImageHolder = new ImageHolder[targetImages.Count()];
			Parallel.For(0,targetImageHolder.Length,pos=>{
				targetImageHolder[pos].OriginalImageData = targetImages.ElementAtOrDefault(pos);
				targetImageHolder[pos].ReducedImageData = targetImages.ElementAtOrDefault(pos)
					.Constrain(new Size(135, 135))
					.EntropyCrop(180)
					.DetectEdges(new Laplacian3X3EdgeFilter(), true);
			});
			List<byte[]> filteredRes = new List<byte[]>();

			Parallel.ForEach(targetImageHolder, act => {
				if (!act.ReducedImageData.DoesImageExist(selfImageHolder.Select(s => s.ReducedImageData), scorePercantage))
				{
					filteredRes.Add(act.OriginalImageData);
				}
			});

			return filteredRes;
		}
		public static byte[] ResizeToClosestAspectRatio (this byte[] image)
		{
			var imageBitmap = image.ByteToBitmap();
			int width = imageBitmap.Width;
			int height = imageBitmap.Height;
			float ogratio = (float) width / height;
			float[] AllowedAspectRatios = new float[] {1.0f, 1.8f, 1.9f, 0.8f };

			float DistnaceCalculate(float a, float b)
			{
				return Math.Abs(a-b);
			}
			var lowestdistance = AllowedAspectRatios.ToList().Select(a =>
			{
				return DistnaceCalculate(ogratio,a);
			}).Min(n=>n);

			var closestRatio = AllowedAspectRatios.ElementAt(AllowedAspectRatios.ToList()
				.FindIndex(s=> DistnaceCalculate(ogratio,s)==lowestdistance));
			int newHeightc = (int) ((float) width / closestRatio);
			return Filters.ResizeImage(image,ResizeMode.Stretch,new Size(width,newHeightc));//.ResizeImage(imageBitmap,width,newHeightc).BitmapToByte();
		}
		public static IEnumerable<ImageHolder> Distinct(this ImageHolder[] imageHolders, ImageHolder[] ogholders, double threshHold)
		{
			List<ImageHolder> results =  new List<ImageHolder>();
			if(imageHolders==null) throw new Exception("image cannot be null");
			var sortTarget = imageHolders
				.Select(s=>ImagePhash.ComputeDigest(s.ReducedImageData.ByteToBitmap().ToLuminanceImage())).ToList();

			sortTarget.Sort();
			var sortOg = ogholders
				.Select(s => ImagePhash.ComputeDigest(s.ReducedImageData.ByteToBitmap().ToLuminanceImage())).ToList();
			sortOg.Sort();

			Parallel.For(0, sortOg.Count, pos =>
			{
				if(ImagePhash.GetCrossCorrelation(sortOg[pos],sortTarget[pos]) < threshHold)
				{
					results.Add(imageHolders[pos]);
				}
				
			});
			return results;
		}
		public static bool DoesImageExist(this byte[] image, IEnumerable<byte[]> images, double scorePercantage)
		{
			if(image == null) throw new Exception("image cannot be null");
			var orginalHash = ImagePhash.ComputeDigest(image.ByteToBitmap().ToLuminanceImage());
			if(orginalHash==null) return false;
			bool res = false;
			Parallel.ForEach(images, (curr, state)=>{
				var bitmap = (Bitmap)curr.ByteToBitmap();
				var Currenthash = ImagePhash.ComputeDigest(bitmap.ToLuminanceImage());
				var score = ImagePhash.GetCrossCorrelation(Currenthash, orginalHash);
				if (score > scorePercantage)
				{
					res = true;
					state.Break();
				}
			});
			return res;
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
					webClient.Proxy = null;
					webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
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
			if(imagesByte==null) return null;
			using (var ms = new MemoryStream(imagesByte))
			{
				return new Bitmap(ms);
			}
		}
		public static byte[] BitmapToByte(this Bitmap bitmap)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				bitmap.Save(ms, ImageFormat.Jpeg);
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
		public static IEnumerable<byte[]> ResizeManyToClosestAspectRatio(this IEnumerable<byte[]>images)
		{
			List<byte[]> resizedImages = new List<byte[]>();
			Parallel.ForEach(images, image =>
			{
				resizedImages.Add(image.ResizeToClosestAspectRatio());
			});
			return resizedImages;
		}
		public static byte[] MostSimilarImage(this Color color, List<byte[]> images)
		{
			List<Color> dom_colors = new List<Color>();

			images.ForEach(resizedImage =>
			{
				var bitmap = resizedImage.ByteToBitmap().ReduceBitmap(380, 380);
				dom_colors.Add(GetDominantColor(bitmap));
			});
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
