using System.Drawing;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Filters.EdgeDetection;
using Quarkless.Analyser.Models;

namespace Quarkless.Analyser
{
	public interface IFilters
	{
		byte[] SharpenImage(byte[] imageData, int size);
		Bitmap ResizeImage(Bitmap image, int width, int height);
		byte[] ResizeImage(byte[] imageData, ResizeMode resizeMode, Size size);
		byte[] DetectEdges(byte[] imageData, IEdgeFilter edgeFilter, bool grayScale);
		byte[] Constrain(byte[] imageData, Size size);
		byte[] EntropyCrop(byte[] imageData, byte threshHold = 128);
		byte[] ApplyFilter(byte[] imageData, FilterType filterType);
	}
}