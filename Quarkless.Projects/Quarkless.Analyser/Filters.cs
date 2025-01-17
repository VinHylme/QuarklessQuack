﻿using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Filters.EdgeDetection;
using ImageProcessor.Imaging.Filters.Photo;
using ImageProcessor.Imaging.Formats;
using Quarkless.Analyser.Models;

namespace Quarkless.Analyser
{
	public class Filters : IFilters
	{
		public byte[] SharpenImage(byte[] imageData, int size)
		{
			var gaussianLayer = new GaussianLayer(size);
			using (var outStream = new MemoryStream())
			{
				using (var factory = new ImageFactory(preserveExifData: true))
				{
					factory.Load(imageData)
						.Format(new JpegFormat() { Quality = 100 })
						.GaussianSharpen(gaussianLayer)
						.Save(outStream);
				}
				return outStream.ToArray();
			}
		}
		public Bitmap ResizeImage(Bitmap image, int width, int height)
		{
			Bitmap output = new Bitmap(width, height, image.PixelFormat);

			using (Graphics g = Graphics.FromImage(output))
			{
				g.CompositingQuality = CompositingQuality.HighQuality;
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = SmoothingMode.HighQuality;

				double ratioW = (double)width / (double)image.Width;
				double ratioH = (double)height / (double)image.Height;
				double ratio = ratioW < ratioH ? ratioW : ratioH;
				int insideWidth = (int)(image.Width * ratio);
				int insideHeight = (int)(image.Height * ratio);

				g.DrawImage(image, new Rectangle((width / 2) - (insideWidth / 2), (height / 2) - (insideHeight / 2), insideWidth, insideHeight));
			}

			return output;
		}
		public byte[] ResizeImage(byte[] imageData, ResizeMode resizeMode, Size size)
		{
			if (imageData == null) return null;
			var resizeLayer = new ResizeLayer(size, resizeMode);
			using (var outStream = new MemoryStream())
			{
				using (var factory = new ImageFactory(preserveExifData: true))
				{
					factory.Load(imageData)
						.Format(new JpegFormat() { Quality = 100 })
						.Resize(resizeLayer)
						.Save(outStream);
				}
				return outStream.ToArray();
			}
		}
		public byte[] DetectEdges(byte[] imageData, IEdgeFilter edgeFilter, bool grayScale)
		{
			if (imageData == null) return null;
			using (var outStream = new MemoryStream())
			{
				using (var factory = new ImageFactory(preserveExifData: true))
				{
					factory.Load(imageData)
						.Format(new JpegFormat() { Quality = 100 })
						.DetectEdges(edgeFilter, grayScale)
						.Save(outStream);
				}
				return outStream.ToArray();
			}
		}
		public byte[] Constrain(byte[] imageData, Size size)
		{
			if (imageData == null) return null;
			using (var outStream = new MemoryStream())
			{
				using (var factory = new ImageFactory(preserveExifData: true))
				{
					factory.Load(imageData)
						.Format(new JpegFormat() { Quality = 100 })
						.Constrain(size)
						.Save(outStream);
				}
				return outStream.ToArray();
			}
		}
		public byte[] EntropyCrop(byte[] imageData, byte threshHold = 128)
		{
			if (imageData == null) return null;
			using (var outStream = new MemoryStream())
			{
				using (var factory = new ImageFactory(preserveExifData: true))
				{
					factory.Load(imageData)
						.Format(new JpegFormat() { Quality = 100 })
						.EntropyCrop(threshHold)
						.Save(outStream);
				}
				return outStream.ToArray();
			}
		}
		public byte[] ApplyFilter(byte[] imageData, FilterType filterType)
		{
			if (imageData == null) return null;
			using (var outStream = new MemoryStream())
			{
				using (var factory = new ImageFactory(preserveExifData: true))
				{
					factory.Load(imageData)
						.Format(new JpegFormat() { Quality = 100 })
						.Filter(
							filterType == FilterType.BlackWhite ? MatrixFilters.BlackWhite :
							filterType == FilterType.GrayScale ? MatrixFilters.GreyScale :
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