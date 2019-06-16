using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Filters.Photo;
using ImageProcessor.Imaging.Formats;
using System.IO;

namespace Quarkless.MediaAnalyser
{
	public static class Filters
	{
		public static byte[] SharpenImage(this byte[] imageData, int size)
		{
			GaussianLayer gaussianLayer = new GaussianLayer(size);
			using (MemoryStream outStream = new MemoryStream())
			{
				using (ImageFactory factory = new ImageFactory(preserveExifData: true))
				{
					factory.Load(imageData)
						.Format(new JpegFormat() { Quality = 100 })
						.GaussianSharpen(gaussianLayer)
						.Save(outStream);
				}
				return outStream.ToArray();
			}
		}
		public static byte[] ApplyFilter(this byte[] imageData, FilterType filterType)
		{
			using(MemoryStream outStream = new MemoryStream())
			{
				using(ImageFactory factory = new ImageFactory(preserveExifData: true))
				{
					factory.Load(imageData)
						.Format(new JpegFormat() { Quality = 100})
						.Filter(
						 filterType == FilterType.BlackWhite ? MatrixFilters.BlackWhite : 
						 filterType == FilterType.Comic ? MatrixFilters.Comic : 
						 filterType == FilterType.HiSatch ? MatrixFilters.HiSatch : 
						 filterType == FilterType.LomoGraph ? MatrixFilters.Lomograph : 
						 filterType == FilterType.LoSatch ? MatrixFilters.LoSatch : 
						 filterType == FilterType.Polaroid ? MatrixFilters.Polaroid : 
						 filterType == FilterType.Sepia ? MatrixFilters.Sepia : null)
						.Save(outStream);
				}
				return outStream.ToArray();
			}
		}
	}
}
