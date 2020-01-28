using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Quarkless.Analyser.Extensions
{
	public static class Helper
	{
		public static void CreateDirectoryIfDoesNotExist(params string[] paths)
		{
			foreach (var path in paths)
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
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
		public static Bitmap ByteToBitmap(this byte[] imagesByte)
		{
			if (imagesByte == null) return null;
			using (var ms = new MemoryStream(imagesByte))
			{
				ms.Seek(0, SeekOrigin.Begin);
				return new Bitmap(ms);
			}
		}
		public static bool IsFileLocked(this FileInfo file)
		{
			FileStream stream = null;

			try
			{
				stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				return true;
			}
			finally
			{
				stream?.Close();
			}
			return false;
		}
		public static IEnumerable<Bitmap> ReadImagesFromDirectory(this string path, string pattern)
		{
			var images = Directory.EnumerateFiles(path, pattern);
			foreach (var image in images)
			{
				var fil = File.ReadAllBytes(image);
				var meme = new MemoryStream(fil);
				yield return new Bitmap(Image.FromStream(meme));
			}
		}
		public static bool IsValidImage(this byte[] bytes)
		{
			try
			{
				using var ms = new MemoryStream(bytes);
				Image.FromStream(ms);
			}
			catch (ArgumentException)
			{
				return false;
			}
			return true;
		}

		#region Manipulators
		public static IEnumerable<byte[]> SelectImageOnSize(this IEnumerable<byte[]> imageData, Size imSize)
		{
			var toreturn = new List<byte[]>();
			foreach (var image in imageData)
			{
				if (image == null) continue;
				try
				{
					var bitmapImage = image.ByteToBitmap();
					if (bitmapImage != null)
					{
						if (bitmapImage.Width > imSize.Width && bitmapImage.Height > imSize.Height)
							toreturn.Add(image);
					}
				}
				catch (Exception ee)
				{
					Console.WriteLine(ee.Message);
				}
			}
			return toreturn;
		}

		public static int ColorDiff(this Color c1, Color c2)
		{
			return (int)Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
								+ (c1.G - c2.G) * (c1.G - c2.G)
								+ (c1.B - c2.B) * (c1.B - c2.B));
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
		public static bool ImageSizeCheckFromByte(this byte[] imageData, Size imSize)
		{
			if (imageData == null) return false;
			try
			{
				var bitmap = imageData.ByteToBitmap();
				if (bitmap != null)
				{
					if (bitmap.Width > imSize.Width && bitmap.Height > imSize.Height)
						return true;
				}
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return false;
			}
			return false;
		}
		public static Dictionary<Color, int> GetColorPercentage(this Bitmap bmp)
		{
			if (bmp == null) throw new ArgumentNullException("null is not allowed");
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
						var color = Color.FromArgb(p[x + 0], p[x + 1], p[x + 2]);
						if (frequency.ContainsKey(color))
						{
							frequency[color]++;
						}
						else
						{
							frequency.Add(color, 1);
						}
					}

					p += stride;
				}
			}
			return frequency;
		}
		public static bool SimilarColors(this IEnumerable<Color> freq, IEnumerable<Color> profileColors, 
			double threshHold = 0)
		{
			var contains = 0;
			foreach (var profileColor in profileColors)
			{
				var diffMin = freq.Select(x => ColorDiff(x, profileColor)).Min(s => s);
				var tcR = profileColor.R + profileColor.G + profileColor.B;
				var targetPercentage = Math.Abs((tcR * threshHold) - tcR);

				if (diffMin < targetPercentage)
				{
					contains++;
				}
			}
			return contains > 0;
		}

		#endregion

		#region Maths
		public static float DistanceCalculate(this float a, float b)
		{
			return Math.Abs(a - b);
		}
		public static bool IsApproximatelyEqualTo(this float initialValue, float value)
		{
			return IsApproximatelyEqualTo(initialValue, value, 0.00001);
		}
		public static bool IsApproximatelyEqualTo(this float initialValue, float value, double maximumDifferenceAllowed)
		{
			// Handle comparisons of floating point values that may not be exactly the same
			return (Math.Abs(initialValue - value) < maximumDifferenceAllowed);
		}
		
		#endregion
	}
}
