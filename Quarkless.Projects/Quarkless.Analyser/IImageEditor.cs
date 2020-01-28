using System.Collections.Generic;
using System.Drawing;
using Quarkless.Analyser.Models;

namespace Quarkless.Analyser
{
	public interface IImageEditor
	{
		bool IsImageGood(byte[] imBytes, IEnumerable<Color> profileColors, double threshHold, Size size);
		IEnumerable<byte[]> ResizeManyToClosestAspectRatio(IEnumerable<byte[]> images);
		byte[] ResizeToClosestAspectRatio(byte[] image);
		double GetClosestAspectRatio(byte[] image);
		Bitmap ReduceBitmap(Bitmap original, int reducedWidth, int reducedHeight);
		byte[] MostSimilarImage(Color color, List<byte[]> images);
		IEnumerable<byte[]> CreateCarousel(byte[] imageData, int splitBy = 3);
		Bitmap CropImage(Bitmap b, Rectangle r);
		bool DoesImageExist(byte[] image, IEnumerable<byte[]> images, double scoreThreshold);
		IEnumerable<ImageHolder> Distinct(ImageHolder[] imageHolders, ImageHolder[] originalImageData, double threshHold);
		double GetAspectRatio(byte[] image);

		IEnumerable<byte[]> RemoveDuplicateImages(IEnumerable<byte[]> selfImages,
			IEnumerable<byte[]> targetImages, double scoreThreshold);

		IEnumerable<byte[]> DistinctImages(IEnumerable<byte[]> images, double score = 0.75);
		bool ImageIsDuplicate(byte[] image, byte[] targetImage, double scoreThreshold);
		IEnumerable<ImageHolder> DuplicateImages(IEnumerable<byte[]> images, double score = 0.90);
	}
}