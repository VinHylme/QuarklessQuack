using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET;
using Microsoft.Extensions.Options;

namespace Quarkless.MediaAnalyser
{
	public class PostAnalyser : IPostAnalyser
	{
		private readonly string _tempVideoPath;
		private readonly string _tempImagePath;
		private readonly string _ffmpegEnginePath;
		public PostAnalyser(IOptions<MediaAnalyserOptions> options)
		{
			if(options.Value.Equals(null))
				throw new NullReferenceException();
			_ffmpegEnginePath = options.Value.FfmpegEnginePath;
			_tempImagePath = options.Value.TempImagePath;
			_tempVideoPath = options.Value.TempVideoPath;
		}

		public async Task<byte[]> GenerateVideoThumbnail(byte[] video, int specificFrame = 5)
		{
			var videoPath = string.Format(_tempVideoPath + "video_{0}_{1}.mp4", Guid.NewGuid(), DateTime.UtcNow.Ticks);
			var imagePath = string.Format(_tempImagePath + "image_{0}_{1}.jpeg", Guid.NewGuid(), DateTime.UtcNow.Ticks);

			File.WriteAllBytes(videoPath, video);
			try
			{
				var mediaFile = new MediaFile(videoPath);
				var engine = new Engine(_ffmpegEnginePath);
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
		public async Task<string> IsVideoSimilar(Color profileColor, byte[] video, double threshHold, int frameSkip = 5)
		{
			var videoPath = string.Format(_tempVideoPath + "video_{0}.mp4", Guid.NewGuid());
			var domColor = new Color();
			File.WriteAllBytes(videoPath, video);
			try
			{
				var mediaFile = new MediaFile(videoPath);
				var engine = new Engine(_ffmpegEnginePath);
				var meta = engine.GetMetaDataAsync(mediaFile).GetAwaiter().GetResult();
				var i = 0;
				while (i < meta.Duration.Seconds)
				{
					var opt = new ConversionOptions { Seek = TimeSpan.FromSeconds(frameSkip) };
					var outputFile = new MediaFile(($@"{_tempImagePath}image-{i}_{Guid.NewGuid()}.jpeg"));
					await engine.GetThumbnailAsync(mediaFile, outputFile, opt);
					i++;
				}
				var videoFrames =  _tempImagePath.ReadImagesFromDirectory("*.jpeg").ToList();
				var videoColorsAvg = new List<Color>();

				videoFrames.ForEach(im => videoColorsAvg.Add(im.GetDominantColor()));
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

			var colorDiffs = domColor.ColorDiff(profileColor);
			var maximus = profileColor.R + profileColor.G + profileColor.B;
			var targetPerc = Math.Abs((maximus * threshHold) - maximus);

			if (colorDiffs < targetPerc)
			{
				return videoPath;
			}
			DisposeVideos(videoPath);
			return null;
		}

		public bool IsImageGood(byte[] imBytes, IEnumerable<Color> profileColors, double threshHold, Size size)
		{
			if (imBytes == null) return false;

			if (!imBytes.ImageSizeCheckFromByte(size)) return false;

			var colorFrequency = imBytes.ByteToBitmap().GetColorPercentage().OrderBy(_ => _.Value);

			var color = colorFrequency.Take(5).Select(x => x.Key)
				.SimilarColors(profileColors, threshHold / 100);
			return color;
		}
		public async Task<bool> IsVideoGood(byte[] bytes, Color profileColor, double threshHold, int frameSkip = 5)
		{
			if (bytes == null) return false;
			var simPath = await IsVideoSimilar(profileColor, bytes, threshHold, frameSkip);
			return !string.IsNullOrEmpty(simPath);
		}

		public void DisposeVideos(string loc = null, int retries = 8)
		{
			try
			{
				if (string.IsNullOrEmpty(loc))
				{
					Directory.EnumerateFiles(_tempImagePath, "*.jpeg").ToList()
						.ForEach(f => {
							var fileInfo = new FileInfo(f);
							for (var tries = 0; IsFileLocked(fileInfo) && tries < 8; tries++)
							{
								Thread.Sleep(1000);
							}
							fileInfo.Delete();
						});

					Directory.EnumerateFiles(_tempVideoPath, "*.mp4").ToList()
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
					for (var tries = 0; IsFileLocked(fileInfo) && tries < retries; tries++)
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
		public bool IsFileLocked(FileInfo file)
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
	}
}
