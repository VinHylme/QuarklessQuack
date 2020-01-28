using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Filters.EdgeDetection;
using Quarkless.Analyser.Extensions;
using Quarkless.Analyser.Models;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;

namespace Quarkless.Analyser
{
	public class ImageEditor : IImageEditor
	{
		private static IFilters Filters => new Filters();
		public bool IsImageGood(byte[] imBytes, IEnumerable<Color> profileColors, double threshHold, Size size)
		{
			if (imBytes == null) return false;

			if (!imBytes.ImageSizeCheckFromByte(size)) return false;

			var colorFrequency = imBytes.ByteToBitmap().GetColorPercentage().OrderBy(_ => _.Value);

			var color = colorFrequency.Take(5).Select(x => x.Key)
				.SimilarColors(profileColors, threshHold / 100);
			return color;
		}
		public IEnumerable<byte[]> ResizeManyToClosestAspectRatio(IEnumerable<byte[]> images)
		{
			var resizedImages = new List<byte[]>();
			Parallel.ForEach(images, image =>
			{
				resizedImages.Add(ResizeToClosestAspectRatio(image));
			});
			return resizedImages;
		}
		public byte[] ResizeToClosestAspectRatio(byte[] image)
		{
			var imageBitmap = image.ByteToBitmap();
			var width = imageBitmap.Width;
			var height = imageBitmap.Height;
			var ratio = (float)width / height;
			var allowedAspectRatios = new[] { 1.0f, 1.8f, 1.9f, 0.8f };
			var outdistance = allowedAspectRatios.ToList().Select(a => ratio.DistanceCalculate(a)).Min(n => n);
			
			var closestRatio = allowedAspectRatios.ElementAt(allowedAspectRatios.ToList()
				.FindIndex(s => ratio.DistanceCalculate(s).IsApproximatelyEqualTo(outdistance)));

			var newHeight = (int)(width / closestRatio);
			return Filters.ResizeImage(image,ResizeMode.Stretch, new Size(width, newHeight));//.ResizeImage(imageBitmap,width,newHeightc).BitmapToByte();
		}
		public double GetClosestAspectRatio(byte[] image)
		{
			var imageBitmap = image.ByteToBitmap();
			var aspect = (float)((double)imageBitmap.Width / (double)imageBitmap.Height);
			var allowedAspectRatios = new float[] { 1.0f, 1.8f, 1.9f, 0.8f };
			var dist = allowedAspectRatios.Select(s => aspect.DistanceCalculate(s)).Min(x => x);
			return allowedAspectRatios.ElementAt(allowedAspectRatios.ToList().FindIndex(s => aspect.DistanceCalculate(s).IsApproximatelyEqualTo(dist)));
		}
		public Bitmap ReduceBitmap(Bitmap original, int reducedWidth, int reducedHeight)
		{
			var reduced = new Bitmap(reducedWidth, reducedHeight);
			using (var dc = Graphics.FromImage(reduced))
			{
				dc.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				dc.DrawImage(original, new Rectangle(0, 0, reducedWidth, reducedHeight), new Rectangle(0, 0, original.Width, original.Height), GraphicsUnit.Pixel);
			}

			return reduced;
		}
		public byte[] MostSimilarImage(Color color, List<byte[]> images)
		{
			var domColors = new List<Color>();

			images.ForEach(resizedImage =>
			{
				var bitmap = ReduceBitmap(resizedImage.ByteToBitmap(),380, 380);
				domColors.Add(bitmap.GetDominantColor());
			});
			var colorDiffs = domColors.Select(n => n.ColorDiff(color)).Min(n => n);
			var pos = domColors.FindIndex(n => n.ColorDiff(color) == colorDiffs);
			var sel = images.ElementAtOrDefault(pos);
			return sel;
		}
		public IEnumerable<byte[]> CreateCarousel(byte[] imageData, int splitBy = 3)
		{
			Bitmap original = null;
			Bitmap temp = null;
			var bundle = new List<byte[]>();

			var toImage = imageData.ByteToBitmap();
			var width = toImage.Width;
			//eg run
			//image of 1280 x 720 -> 1280/3 = 426.66
			var widthOfSquare = (int)Math.Truncate((float)width / (float)splitBy);
			var startX = 0;

			for (var i = 0; i < splitBy; i++)
			{
				try
				{
					original = imageData.ByteToBitmap();
					var rect = new Rectangle(startX, 0, widthOfSquare, toImage.Height);
					temp = CropImage(original,rect);
					bundle.Add(temp.BitmapToByte());

					startX += widthOfSquare;
				}
				finally
				{
					original?.Dispose();
					temp?.Dispose();
				}
			}
			return bundle;
		}
		public Bitmap CropImage(Bitmap b, Rectangle r)
		{
			var nb = new Bitmap(r.Width, r.Height);
			var g = Graphics.FromImage(nb);
			g.DrawImage(b, -r.X, -r.Y);
			return nb;
		}
		public bool DoesImageExist(byte[] image, IEnumerable<byte[]> images, double scoreThreshold)
		{
			if (image == null) throw new Exception("image cannot be null");
			var computeDigest = ImagePhash.ComputeDigest(image.ByteToBitmap().ToLuminanceImage());
			if (computeDigest == null) return false;
			var res = false;
			Parallel.ForEach(images, (curr, state) => {
				var bitmap = curr.ByteToBitmap();
				var currentHash = ImagePhash.ComputeDigest(bitmap.ToLuminanceImage());
				var score = ImagePhash.GetCrossCorrelation(currentHash, computeDigest);
				if (!(score > scoreThreshold)) return;
				res = true;
				state.Break();
			});
			return res;
		}
		public IEnumerable<ImageHolder> Distinct(ImageHolder[] imageHolders, ImageHolder[] originalImageData, double threshHold)
		{
			var results = new List<ImageHolder>();
			if (imageHolders == null) throw new Exception("image cannot be null");
			var sortTarget = imageHolders
				.Select(s => ImagePhash.ComputeDigest(s.ReducedImageData.ByteToBitmap().ToLuminanceImage())).ToList();

			sortTarget.Sort();
			var sortOg = originalImageData
				.Select(s => ImagePhash.ComputeDigest(s.ReducedImageData.ByteToBitmap().ToLuminanceImage())).ToList();
			sortOg.Sort();

			Parallel.For(0, sortOg.Count, pos =>
			{
				if (ImagePhash.GetCrossCorrelation(sortOg[pos], sortTarget[pos]) < threshHold)
				{
					results.Add(imageHolders[pos]);
				}

			});
			return results;
		}
		public double GetAspectRatio(byte[] image)
		{
			var imageBitmap = image.ByteToBitmap();
			return (double)((double)imageBitmap.Width / (double)imageBitmap.Height);
		}
		public IEnumerable<byte[]> RemoveDuplicateImages(IEnumerable<byte[]> selfImages,
			IEnumerable<byte[]> targetImages, double scoreThreshold)
		{
			var enumerable = selfImages as byte[][] ?? selfImages.ToArray();
			if (!enumerable.Any()) throw new Exception("self images cannot be empty");
			var images = targetImages as byte[][] ?? targetImages.ToArray();
			if (!images.Any()) throw new Exception("target images cannot be empty");
			targetImages = images.SelectImageOnSize(new Size(600, 600));
			//initialise
			var selfImageHolder = new ImageHolder[enumerable.Count()];
			Parallel.For(0, selfImageHolder.Length, pos =>
			{
				selfImageHolder[pos].OriginalImageData = enumerable.ElementAtOrDefault(pos);
				selfImageHolder[pos].ReducedImageData = Filters.DetectEdges(
					Filters.EntropyCrop(
						Filters.Constrain(enumerable.ElementAtOrDefault(pos), 
							new Size(135, 135)), 180), 
					new Laplacian3X3EdgeFilter(),
					true);
			});

			var targetImageHolder = new ImageHolder[images.Count()];
			Parallel.For(0, targetImageHolder.Length, pos => {
				targetImageHolder[pos].OriginalImageData = images.ElementAtOrDefault(pos);

				targetImageHolder[pos].ReducedImageData = Filters.DetectEdges(
					Filters.EntropyCrop(
						Filters.Constrain(enumerable.ElementAtOrDefault(pos),
							new Size(135, 135)), 180),
					new Laplacian3X3EdgeFilter(),
					true);
			});
			var filteredRes = new List<byte[]>();

			Parallel.ForEach(targetImageHolder, act => {
				if (!DoesImageExist(act.ReducedImageData, selfImageHolder.Select(s => s.ReducedImageData), scoreThreshold))
				{
					filteredRes.Add(act.OriginalImageData);
				}
			});

			return filteredRes;
		}
		public IEnumerable<byte[]> DistinctImages(IEnumerable<byte[]> images, double score = 0.75)
		{
			var enumerable = images as byte[][] ?? images.ToArray();
			if (!enumerable.Any()) return null;
			var imageHolder = new ImageHolder[enumerable.Count()];
			Parallel.For(0, imageHolder.Length, pos =>
			{
				imageHolder[pos].OriginalImageData = enumerable.ElementAtOrDefault(pos);
				imageHolder[pos].ReducedImageData = Filters.DetectEdges(
					Filters.EntropyCrop(
						Filters.Constrain(enumerable.ElementAtOrDefault(pos),
							new Size(135, 135)), 180),
					new Laplacian3X3EdgeFilter(),
					true);
			});
			var results = new List<byte[]>();
			for (var posX = 0; posX < imageHolder.Length; posX++)
			{
				var isDuplicate = false;
				for (var posY = 1; posY < imageHolder.Length; posY++)
				{
					if (!ImageIsDuplicate(imageHolder[posX].ReducedImageData,imageHolder[posY].ReducedImageData, score)) continue;
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
		public bool ImageIsDuplicate(byte[] image, byte[] targetImage, double scoreThreshold)
		{
			var ogHash = ImagePhash.ComputeDigest(image.ByteToBitmap().ToLuminanceImage());
			if (targetImage == null) return false;
			var targetHash = ImagePhash.ComputeDigest(targetImage.ByteToBitmap().ToLuminanceImage());
			return ImagePhash.GetCrossCorrelation(ogHash, targetHash) > scoreThreshold;
		}
		public IEnumerable<ImageHolder> DuplicateImages(IEnumerable<byte[]> images, double score = 0.90)
		{
			var enumerable = images as byte[][] ?? images.ToArray();
			if (!enumerable.Any()) return null;
			var imageHolder = new ImageHolder[enumerable.Count()];
			Parallel.For(0, imageHolder.Length, pos =>
			{
				imageHolder[pos].OriginalImageData = enumerable.ElementAtOrDefault(pos);
				imageHolder[pos].ReducedImageData = Filters.DetectEdges(
					Filters.EntropyCrop(
						Filters.Constrain(enumerable.ElementAtOrDefault(pos),
							new Size(135, 135)), 180),
					new Laplacian3X3EdgeFilter(),
					true);
			});
			var results = new List<ImageHolder>();
			for (var posX = 0; posX < imageHolder.Length; posX++)
			{
				var isDuplicate = false;
				for (var posY = 1; posY < imageHolder.Length; posY++)
				{
					if (posY == posX) continue;
					if (!ImageIsDuplicate(imageHolder[posX].ReducedImageData, imageHolder[posY].ReducedImageData, score)) continue;
					isDuplicate = true;
					break;
				};
				if (isDuplicate)
				{
					results.Add(imageHolder[posX]);
				}
			};
			return results;
		}
	}
}