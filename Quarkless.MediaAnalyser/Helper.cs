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
using System.Threading;

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
			var settingPath = @"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Quarkless";
			IConfiguration configuration = new ConfigurationBuilder().
				SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();
			return configuration["MediaPath:" + name];
		}
		public static bool ImageIsDuplicate(this byte[] image, byte[] targetImage, double scorePerctange)
		{
			var ogHash = ImagePhash.ComputeDigest(image.ByteToBitmap().ToLuminanceImage());
			if(targetImage==null) return false;
			var targetHash = ImagePhash.ComputeDigest(targetImage.ByteToBitmap().ToLuminanceImage());
			return ImagePhash.GetCrossCorrelation(ogHash,targetHash) > scorePerctange;
		}
		public static bool ImageSizeCheckFromByte(this byte[] imageData, Size imSize)
		{
			if (imageData != null)
			{
				try
				{
					var bitmap = imageData.ByteToBitmap();
					if (bitmap != null)
					{
						if(bitmap.Width > imSize.Width && bitmap.Height > imSize.Height)
							return true;
					}
				}
				catch(Exception ee)
				{
					Console.WriteLine(ee.Message);
					return false;
				}
			}
			return false;
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
		public static IEnumerable<byte[]> DuplicateImages(this IEnumerable<byte[]> images, double score = 0.90)
		{
			if (images.Count() <= 0) return null;
			ImageHolder[] imageHolder = new ImageHolder[images.Count()];
			Parallel.For(0, imageHolder.Length, pos =>
			{
				imageHolder[pos].OriginalImageData = images.ElementAtOrDefault(pos);
				imageHolder[pos].ReducedImageData = images.ElementAtOrDefault(pos)
				.Constrain(new Size(400, 400))
				.EntropyCrop(125)
				.DetectEdges(new Laplacian3X3EdgeFilter(), true);
			});
			List<byte[]> results = new List<byte[]>();
			for (int posX = 0; posX < imageHolder.Length; posX++)
			{
				bool isDuplicate = false;
				for (int posY = 1; posY < imageHolder.Length; posY++)
				{
					if (posY != posX) { 
						if (imageHolder[posX].ReducedImageData.ImageIsDuplicate(imageHolder[posY].ReducedImageData, score))
						{
							isDuplicate = true;
							break;
						}
					}
				};
				if (isDuplicate)
				{
					results.Add(imageHolder[posX].OriginalImageData);
				}
			};
			return results;
		}

		public static IEnumerable<byte[]> DistinctImages(this IEnumerable<byte[]> images, double score = 0.75)
		{
			if(images.Count()<=0) return null;
			ImageHolder[] imageHolder = new ImageHolder[images.Count()];
			Parallel.For(0, imageHolder.Length, pos =>
			{
				imageHolder[pos].OriginalImageData = images.ElementAtOrDefault(pos);
				imageHolder[pos].ReducedImageData = images.ElementAtOrDefault(pos)
				.Constrain(new Size(200,200))
				.EntropyCrop(180)
				.DetectEdges(new Laplacian3X3EdgeFilter(),true);
			});
			List<byte[]> results = new List<byte[]>();
			for(int posX = 0; posX < imageHolder.Length; posX++) 
			{
				bool isDuplicate = false;
				for (int posY = 1; posY < imageHolder.Length; posY++)
				{
					if (imageHolder[posX].ReducedImageData.ImageIsDuplicate(imageHolder[posY].ReducedImageData, score))
					{
						isDuplicate = true;
						break;
					}
				};
				if (!isDuplicate)
				{
					results.Add(imageHolder[posX].OriginalImageData);
				}
			};
			return results;
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
		public static double GetAspectRatio(this byte[] image)
		{
			var imagebit = image.ByteToBitmap();
			return (double)((double)imagebit.Width / (double) imagebit.Height);
		}
		private static float DistnaceCalculate(float a, float b)
		{
			return Math.Abs(a - b);
		}
		public static double GetClosestAspectRatio(this byte[] image)
		{
			var imagebit = image.ByteToBitmap();
			float aspect = (float) ((double) imagebit.Width / (double) imagebit.Height);
			float[] AllowedAspectRatios = new float[] { 1.0f, 1.8f, 1.9f, 0.8f };
			var dist = AllowedAspectRatios.Select(s=>DistnaceCalculate(aspect,s)).Min(x=>x);
			return AllowedAspectRatios.ElementAt(AllowedAspectRatios.ToList().FindIndex(s=>DistnaceCalculate(aspect,s) == dist));
		}
		public static byte[] ResizeToClosestAspectRatio (this byte[] image)
		{
			var imageBitmap = image.ByteToBitmap();
			int width = imageBitmap.Width;
			int height = imageBitmap.Height;
			float ogratio = (float) width / height;
			float[] AllowedAspectRatios = new float[] {1.0f, 1.8f, 1.9f, 0.8f };

	
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
		public static Bitmap cropAtRect(this Bitmap b, Rectangle r)
		{
			Bitmap nb = new Bitmap(r.Width, r.Height);
			Graphics g = Graphics.FromImage(nb);
			g.DrawImage(b, -r.X, -r.Y);
			return nb;
		}
		public static IEnumerable<byte[]> CreateCarousel(this byte[] imageData, int splitBy = 3)
		{
			Bitmap original = null;
			Bitmap temp = null;
			List<byte[]> bundle = new List<byte[]>();

			var toImage = imageData.ByteToBitmap();
			var width = toImage.Width;
			//eg run
			//image of 1280 x 720 -> 1280/3 = 426.66
			int widthOfSquare = (int) Math.Truncate((float)width / (float)splitBy);
			int startX = 0;

			for(int i = 0; i < splitBy; i++)
			{
				try { 
					original = imageData.ByteToBitmap();
					var rect = new Rectangle(startX, 0, widthOfSquare, toImage.Height);
					temp = original.cropAtRect(rect);
					bundle.Add(temp.BitmapToByte());

					startX+=widthOfSquare;
				}
				finally
				{
					if(original!=null)
						original.Dispose();
					if(temp!=null)
						temp.Dispose();
				}
			}
			return bundle;
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
				ms.Close();
				return ms.ToArray();
			}
		}
		public static Dictionary<Color,int> GetColorPercentage(this Bitmap bmp)
		{
			if(bmp == null) throw new ArgumentNullException("null is not allowed");
			BitmapData srcData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
			Dictionary<Color, int> frequency = new Dictionary<Color, int>();
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
						Color color = Color.FromArgb(p[x+0],p[x+1],p[x+2]);
						if (frequency.ContainsKey(color))
						{
							frequency[color]++;
						}
						else
						{
							frequency.Add(color,1);
						}
					}

					p += stride;
				}
			}
			return frequency;
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
		public static bool SimilarColors(this IEnumerable<Color> @freq, IEnumerable<Color> target, double threshhold = 0)
		{
			int contains = 0;
			foreach (var tc in target)
			{
				var diffMin = freq.Select(x=>ColorDiff(x,tc)).Min(s=>s);
				var maximus = tc.R + tc.G + tc.B;
				var targetPerc = Math.Abs((maximus * threshhold) - maximus);
				if (diffMin < targetPerc)
				{
					contains++;
				}
			}
			return contains > 0;
		}
		public static int ColorDiff(this Color c1, Color c2)
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
		public static string IsVideoSimilar(this Color profileColor, byte[] video, double threshHold, int frameSkip = 5)
		{
			string path = GetFilePathByName("videosTempPath");
			string outp = GetFilePathByName("imagesTempPath");
			string engine_path = GetFilePathByName("enginePath");
			string videoPath = string.Format(path + "video_{0}_{1}.mp4", Guid.NewGuid(), DateTime.UtcNow.ToLongDateString());
			Color dom_color = new Color();
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
					dom_color = Color.FromArgb(r / total, b / total, g / total);
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}

			var colorDiffs = ColorDiff(dom_color, profileColor);
			var maximus = profileColor.R + profileColor.G + profileColor.B;
			var targetPerc = Math.Abs((maximus * threshHold) - maximus);

			if (colorDiffs < targetPerc)
			{
				return videoPath;
			}

			return null;
		}
		public static void DisposeVideos(string loc = null, int retries = 8)
		{
			try
			{
				if (string.IsNullOrEmpty(loc)) { 
					string path = GetFilePathByName("videosTempPath");
					string outp = GetFilePathByName("imagesTempPath");

					Directory.EnumerateFiles(outp, "*.jpeg").ToList()
						.ForEach(f => {
							FileInfo fileInfo = new FileInfo(f);
							for (int tries = 0; IsFileLocked(fileInfo) && tries < 8; tries++)
							{
								Thread.Sleep(1000);
							}
							fileInfo.Delete();
						});

					Directory.EnumerateFiles(path, "*.mp4").ToList()
					.ForEach(f => {
					FileInfo fileInfo = new FileInfo(f);
						for (int tries = 0; IsFileLocked(fileInfo) && tries < retries; tries++)
						{
							Thread.Sleep(1000);
						}
					fileInfo.Delete();
					});	
				}
				else
				{
					FileInfo fileInfo = new FileInfo(loc);
					for(int tries = 0; IsFileLocked(fileInfo) && tries < retries; tries++)
					{
						Thread.Sleep(1000);
					}
					fileInfo.Delete();
				}
			}
			catch (IOException exception)
			{
				Console.WriteLine(string.Format("File locked: {0}", exception));
			}
		
		}
		static bool IsFileLocked(FileInfo file)
		{
			FileStream stream = null;

			try
			{
				stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			//file is not locked
			return false;
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
