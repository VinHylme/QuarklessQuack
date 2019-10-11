using FFmpeg.NET;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
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
		public static bool IsValidImage(this byte[] bytes)
		{
			try {
				using(var ms = new MemoryStream(bytes))
					Image.FromStream(ms);
			}
			catch (ArgumentException) {
				return false;
			}
			return true; 
		}
		private static string GetFilePathByName(string name)
		{
			var settingPath = Path.GetFullPath(Path.Combine(@"..\..\..\..\Quarkless"));
			IConfiguration configuration = new ConfigurationBuilder().
				SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();

			return Path.GetFullPath(@"..\..\..\..\"+configuration["MediaPath:" + name]);
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
			if (imageData == null) return false;
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
			return false;
		}
		public static IEnumerable<byte[]> SelectImageOnSize(this IEnumerable<byte[]> imageData, Size imSize)
		{
			var toreturn = new List<byte[]>();
			foreach(var image in imageData)
			{
				if (image == null) continue;
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
				}
			}
			return toreturn;
		}
		public static IEnumerable<byte[]> DuplicateImages(this IEnumerable<byte[]> images, double score = 0.90)
		{
			var enumerable = images as byte[][] ?? images.ToArray();
			if (!enumerable.Any()) return null;
			var imageHolder = new ImageHolder[enumerable.Count()];
			Parallel.For(0, imageHolder.Length, pos =>
			{
				imageHolder[pos].OriginalImageData = enumerable.ElementAtOrDefault(pos);
				imageHolder[pos].ReducedImageData = enumerable.ElementAtOrDefault(pos)
				.Constrain(new Size(400, 400))
				.EntropyCrop(125)
				.DetectEdges(new Laplacian3X3EdgeFilter(), true);
			});
			var results = new List<byte[]>();
			for (var posX = 0; posX < imageHolder.Length; posX++)
			{
				var isDuplicate = false;
				for (var posY = 1; posY < imageHolder.Length; posY++)
				{
					if (posY == posX) continue;
					if (!imageHolder[posX].ReducedImageData
						.ImageIsDuplicate(imageHolder[posY].ReducedImageData, score)) continue;
					isDuplicate = true;
					break;
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
			var enumerable = images as byte[][] ?? images.ToArray();
			if(!enumerable.Any()) return null;
			var imageHolder = new ImageHolder[enumerable.Count()];
			Parallel.For(0, imageHolder.Length, pos =>
			{
				imageHolder[pos].OriginalImageData = enumerable.ElementAtOrDefault(pos);
				imageHolder[pos].ReducedImageData = enumerable.ElementAtOrDefault(pos)
				.Constrain(new Size(200,200))
				.EntropyCrop(180)
				.DetectEdges(new Laplacian3X3EdgeFilter(),true);
			});
			var results = new List<byte[]>();
			for(var posX = 0; posX < imageHolder.Length; posX++) 
			{
				var isDuplicate = false;
				for (var posY = 1; posY < imageHolder.Length; posY++)
				{
					if (!imageHolder[posX].ReducedImageData
						.ImageIsDuplicate(imageHolder[posY].ReducedImageData, score)) continue;
					isDuplicate = true;
					break;
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
			var enumerable = selfImages as byte[][] ?? selfImages.ToArray();
			if(!enumerable.Any()) throw new Exception("self images cannot be empty");
			var byteses = targetImages as byte[][] ?? targetImages.ToArray();
			if(!byteses.Any()) throw new Exception("target images cannot be empty");
			targetImages = byteses.SelectImageOnSize(new Size(600,600));
			//initialise
			var selfImageHolder = new ImageHolder[enumerable.Count()];
			Parallel.For(0, selfImageHolder.Length, pos =>
			{
				selfImageHolder[pos].OriginalImageData = enumerable.ElementAtOrDefault(pos);
				selfImageHolder[pos].ReducedImageData = enumerable.ElementAtOrDefault(pos)
					.Constrain(new Size(135, 135))
					.EntropyCrop(180)
					.DetectEdges(new Laplacian3X3EdgeFilter(), true);
			});

			var targetImageHolder = new ImageHolder[byteses.Count()];
			Parallel.For(0,targetImageHolder.Length,pos=>{
				targetImageHolder[pos].OriginalImageData = byteses.ElementAtOrDefault(pos);
				targetImageHolder[pos].ReducedImageData = byteses.ElementAtOrDefault(pos)
					.Constrain(new Size(135, 135))
					.EntropyCrop(180)
					.DetectEdges(new Laplacian3X3EdgeFilter(), true);
			});
			var filteredRes = new List<byte[]>();

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
		private static float DistanceCalculate(float a, float b)
		{
			return Math.Abs(a - b);
		}
		public static double GetClosestAspectRatio(this byte[] image)
		{
			var imagebit = image.ByteToBitmap();
			var aspect = (float) ((double) imagebit.Width / (double) imagebit.Height);
			var allowedAspectRatios = new float[] { 1.0f, 1.8f, 1.9f, 0.8f };
			var dist = allowedAspectRatios.Select(s=>DistanceCalculate(aspect,s)).Min(x=>x);
			return allowedAspectRatios.ElementAt(allowedAspectRatios.ToList().FindIndex(s=>DistanceCalculate(aspect,s) == dist));
		}
		public static byte[] ResizeToClosestAspectRatio (this byte[] image)
		{
			var imageBitmap = image.ByteToBitmap();
			var width = imageBitmap.Width;
			var height = imageBitmap.Height;
			var ogratio = (float) width / height;
			var AllowedAspectRatios = new float[] {1.0f, 1.8f, 1.9f, 0.8f };

	
			var lowestdistance = AllowedAspectRatios.ToList().Select(a => DistanceCalculate(ogratio,a)).Min(n=>n);

			var closestRatio = AllowedAspectRatios.ElementAt(AllowedAspectRatios.ToList()
				.FindIndex(s=> DistanceCalculate(ogratio,s)==lowestdistance));
			var newHeightc = (int) ((float) width / closestRatio);
			return image.ResizeImage(ResizeMode.Stretch,new Size(width,newHeightc));//.ResizeImage(imageBitmap,width,newHeightc).BitmapToByte();
		}
		public static IEnumerable<ImageHolder> Distinct(this ImageHolder[] imageHolders, ImageHolder[] ogholders, double threshHold)
		{
			var results =  new List<ImageHolder>();
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
			var res = false;
			Parallel.ForEach(images, (curr, state)=>{
				var bitmap = (Bitmap)curr.ByteToBitmap();
				var Currenthash = ImagePhash.ComputeDigest(bitmap.ToLuminanceImage());
				var score = ImagePhash.GetCrossCorrelation(Currenthash, orginalHash);
				if (!(score > scorePercantage)) return;
				res = true;
				state.Break();
			});
			return res;
		}
		public static byte[] DownloadMedias(this List<string> urls, int poz)
		{
			using (var webClient = new WebClient())
			{
				try
				{
					return poz < 0 ? null : webClient.DownloadData(urls.ElementAt(poz));
				}
				catch (Exception e)
				{
					DownloadMedias(urls, poz--);
					return null;
				}
			}
		}
		public static byte[] DownloadMediaLocal(this string url) => File.ReadAllBytes(url);
		public static byte[] DownloadMedia(this string url)
		{
			using (var webClient = new WebClient())
			{
				try
				{
					webClient.Proxy = null;
					//webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
					webClient.Headers.Add("User-Agent: Other"); 
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
			var nb = new Bitmap(r.Width, r.Height);
			var g = Graphics.FromImage(nb);
			g.DrawImage(b, -r.X, -r.Y);
			return nb;
		}
		public static IEnumerable<byte[]> CreateCarousel(this byte[] imageData, int splitBy = 3)
		{
			Bitmap original = null;
			Bitmap temp = null;
			var bundle = new List<byte[]>();

			var toImage = imageData.ByteToBitmap();
			var width = toImage.Width;
			//eg run
			//image of 1280 x 720 -> 1280/3 = 426.66
			var widthOfSquare = (int) Math.Truncate((float)width / (float)splitBy);
			var startX = 0;

			for(var i = 0; i < splitBy; i++)
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
					original?.Dispose();
					temp?.Dispose();
				}
			}
			return bundle;
		}
		public static Bitmap ByteToBitmap(this byte[] imagesByte)
		{
			if(imagesByte==null) return null;
			try { 
				using (var ms = new MemoryStream(imagesByte))
				{
					ms.Seek(0, SeekOrigin.Begin);
					return new Bitmap(ms);
				}
			}
			catch(Exception io)
			{
				Console.WriteLine(io.Message);
				return null;
			}
		}
		public static byte[] BitmapToByte(this Bitmap bitmap)
		{
			using (var ms = new MemoryStream())
			{
				bitmap.Save(ms, ImageFormat.Jpeg);
				ms.Close();
				return ms.ToArray();
			}
		}
		public static Dictionary<Color,int> GetColorPercentage(this Bitmap bmp)
		{
			if(bmp == null) throw new ArgumentNullException("null is not allowed");
			var srcData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
			var frequency = new Dictionary<Color, int>();
			var bytesPerPixel = Image.GetPixelFormatSize(srcData.PixelFormat) / 8;

			var stride = srcData.Stride;

			var scan0 = srcData.Scan0;

			var totals = new long[] { 0, 0, 0 };

			var width = bmp.Width * bytesPerPixel;
			var height = bmp.Height;

			unsafe
			{
				var p = (byte*)(void*)scan0;

				for (var y = 0; y < height; y++)
				{
					for (var x = 0; x < width; x += bytesPerPixel)
					{
						totals[0] += p[x + 0];
						totals[1] += p[x + 1];
						totals[2] += p[x + 2];
						var color = Color.FromArgb(p[x+0],p[x+1],p[x+2]);
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

			var srcData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

			var bytesPerPixel = Image.GetPixelFormatSize(srcData.PixelFormat) / 8;

			var stride = srcData.Stride;

			var scan0 = srcData.Scan0;

			var totals = new long[] { 0, 0, 0 };

			var width = bmp.Width * bytesPerPixel;
			var height = bmp.Height;

			unsafe
			{
				var p = (byte*)(void*)scan0;

				for (var y = 0; y < height; y++)
				{
					for (var x = 0; x < width; x += bytesPerPixel)
					{
						totals[0] += p[x + 0];
						totals[1] += p[x + 1];
						totals[2] += p[x + 2];
					}

					p += stride;
				}
			}

			long pixelCount = bmp.Width * height;

			var avgB = Convert.ToInt32(totals[0] / pixelCount);
			var avgG = Convert.ToInt32(totals[1] / pixelCount);
			var avgR = Convert.ToInt32(totals[2] / pixelCount);

			bmp.UnlockBits(srcData);

			return System.Drawing.Color.FromArgb(avgR, avgG, avgB);
		}
		public static IEnumerable<byte[]> ResizeManyToClosestAspectRatio(this IEnumerable<byte[]>images)
		{
			var resizedImages = new List<byte[]>();
			Parallel.ForEach(images, image =>
			{
				resizedImages.Add(image.ResizeToClosestAspectRatio());
			});
			return resizedImages;
		}
		public static byte[] MostSimilarImage(this Color color, List<byte[]> images)
		{
			var dom_colors = new List<Color>();

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
			var contains = 0;
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
			var images = Directory.EnumerateFiles(path, pattern);
			foreach (var image in images)
			{
				var fil = File.ReadAllBytes(image);
				var meme = new MemoryStream(fil);
				yield return new Bitmap(Image.FromStream(meme));
			}
		}
		public static async Task<byte[]> GenerateVideoThumbnail(this byte[] video, int specificFrame = 5)
		{
			var path = GetFilePathByName("videosTempPath");
			var outputPath = GetFilePathByName("imagesTempPath");
			var enginePath = GetFilePathByName("enginePath");

			var videoPath = string.Format(path + "video_{0}_{1}.mp4", Guid.NewGuid(), DateTime.UtcNow.Ticks);
			var imagePath = string.Format(outputPath + "image_{0}_{1}.jpeg", Guid.NewGuid(), DateTime.UtcNow.Ticks);

			File.WriteAllBytes(videoPath, video);
			try
			{
				var mediaFile = new MediaFile(videoPath);
				var engine = new Engine(enginePath);
				var opt = new ConversionOptions { Seek = TimeSpan.FromSeconds(specificFrame) };
				var outputFile = new MediaFile(imagePath);

				await engine.GetThumbnailAsync(mediaFile, outputFile, opt);

				var image = File.ReadAllBytes(imagePath);

				File.Delete(videoPath);
				File.Delete(imagePath);

				return image;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public static async Task<string> IsVideoSimilar(this Color profileColor, byte[] video, double threshHold, int frameSkip = 5)
		{
			var path = GetFilePathByName("videosTempPath");
			var outputPath = GetFilePathByName("imagesTempPath");
			var enginePath = GetFilePathByName("enginePath");
			var videoPath = string.Format(path + "video_{0}.mp4", Guid.NewGuid());
			var domColor = new Color();
			File.WriteAllBytes(videoPath, video);
			try
			{
				var mediaFile = new MediaFile(videoPath);
				var engine = new Engine(enginePath);
				var meta = engine.GetMetaDataAsync(mediaFile).GetAwaiter().GetResult();
				var i = 0;
				while (i < meta.Duration.Seconds)
				{
					var opt = new ConversionOptions { Seek = TimeSpan.FromSeconds(frameSkip) };
					var outputFile = new MediaFile(($@"{outputPath}image-{i}_{Guid.NewGuid()}.jpeg"));
					await engine.GetThumbnailAsync(mediaFile, outputFile, opt);
					i++;
				}
				var videoFrames = ReadImagesFromDirectory(outputPath, "*.jpeg").ToList();
				var videoColorsAvg = new List<Color>();

				videoFrames.ForEach(im => videoColorsAvg.Add(GetDominantColor(im)));
				int r = 0, b = 0, g = 0;
				videoColorsAvg.ForEach(c => {
					r += c.R;
					b += c.B;
					g += c.G;
				});
				var total = videoColorsAvg.Count;
				domColor = Color.FromArgb(r / total, b / total, g / total);
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}

			var colorDiffs = ColorDiff(domColor, profileColor);
			var maximus = profileColor.R + profileColor.G + profileColor.B;
			var targetPerc = Math.Abs((maximus * threshHold) - maximus);

			if (colorDiffs < targetPerc)
			{
				return videoPath;
			}
			DisposeVideos(videoPath);
			return null;
		}
		public static void DisposeVideos(string loc = null, int retries = 8)
		{
			try
			{
				if (string.IsNullOrEmpty(loc)) { 
					var path = GetFilePathByName("videosTempPath");
					var outp = GetFilePathByName("imagesTempPath");

					Directory.EnumerateFiles(outp, "*.jpeg").ToList()
						.ForEach(f => {
							var fileInfo = new FileInfo(f);
							for (var tries = 0; IsFileLocked(fileInfo) && tries < 8; tries++)
							{
								Thread.Sleep(1000);
							}
							fileInfo.Delete();
						});

					Directory.EnumerateFiles(path, "*.mp4").ToList()
					.ForEach(f => {
						var fileInfo = new FileInfo(f);
						for (var tries = 0; IsFileLocked(fileInfo) && tries < retries; tries++)
						{
							Thread.Sleep(1000);
						}
						fileInfo.Delete();
					});	
				}
				else
				{
					var fileInfo = new FileInfo(loc);
					for(var tries = 0; IsFileLocked(fileInfo) && tries < retries; tries++)
					{
						Thread.Sleep(1000);
					}
					fileInfo.Delete();
				}
			}
			catch (IOException exception)
			{
				Console.WriteLine($"File locked: {exception}");
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
				stream?.Close();
			}

			//file is not locked
			return false;
		}
		public static byte[] MostSimilarVideo(this Color profileColor, List<byte[]> videos, int frameSkip = 5)
		{
			var path = GetFilePathByName("videosTempPath");
			var outp = GetFilePathByName("imagesTempPath");
			var engine_path = GetFilePathByName("enginePath");
			var videoPath = string.Format(path + "video_{0}_{1}.mp4", Guid.NewGuid(), DateTime.UtcNow.ToLongDateString());
			var dom_colors = new List<Color>();

			foreach (var video in videos)
			{
				File.WriteAllBytes(videoPath, video);
				try
				{
					var mediaFile = new MediaFile(videoPath);
					var engine = new Engine(engine_path);
					var meta = engine.GetMetaDataAsync(mediaFile).GetAwaiter().GetResult();
					var i = 0;
					while (i < meta.Duration.Seconds)
					{
						var opt = new ConversionOptions { Seek = TimeSpan.FromSeconds(frameSkip) };
						var outputFile = new MediaFile((
							$@"{outp}image-{i}_{Guid.NewGuid()}_{DateTime.UtcNow.ToLongDateString()}.jpeg"));
						engine.GetThumbnailAsync(mediaFile, outputFile, opt);
						i++;
					}
					var videoframes = ReadImagesFromDirectory(outp, "*.jpeg").ToList();
					var video_colors_avg = new List<Color>();

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
				catch (Exception ee)
				{
					Console.WriteLine(ee.Message);
					return null;
				}
			}
			var colorDiffs = dom_colors.Select(n => ColorDiff(n, profileColor)).Min(n => n);
			var pos = dom_colors.FindIndex(n => ColorDiff(n, profileColor) == colorDiffs);
			Directory.EnumerateFiles(outp, "*.jpeg").ToList().ForEach(File.Delete);
			Directory.EnumerateFiles(path, "*.mp4").ToList().ForEach(File.Delete);
			return videos.ElementAtOrDefault(pos);
		}
	}
}
