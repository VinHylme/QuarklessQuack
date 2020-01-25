using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Quarkless.Analyser.Extensions;

namespace Quarkless.Analyser
{
	public class VideoEditor : IVideoEditor
	{
		private readonly IFfmpegWrapper _ffmpegWrapper;
		public VideoEditor(IFfmpegWrapper ffmpegWrapper)
		{
			_ffmpegWrapper = ffmpegWrapper;
		}

		private TimeSpan GetVideoDuration(string videoPath)
		{
			var results = _ffmpegWrapper.RunProcess($"-i {videoPath}");
			if (string.IsNullOrEmpty(results))
				return TimeSpan.Zero;
			var durationString = new Regex(@"Duration:[\s\d\:\.]*")
				.Matches(results)
				.FirstOrDefault()
				?.Value.Split(": ")[1];

			return string.IsNullOrEmpty(durationString) ? TimeSpan.Zero : TimeSpan.Parse(durationString);
		}
		private bool GetAllVideoThumbnailFromProcess(string videoPath, string output)
		{
			try
			{
				_ffmpegWrapper.RunProcess($"-i {videoPath} -vf fps=1 {output} -hide_banner");
				return true;
			}
			catch
			{
				return false;
			}
		}
		private bool GetVideoThumbnailFromProcess(string videoPath, string outPutPath, int frame)
		{
			try
			{
				_ffmpegWrapper.RunProcess($"-i {videoPath} -ss {frame} -vframes 1 {outPutPath}");
				return true;
			}
			catch
			{
				return false;
			}
		}
		public string GetPathByFolderName(string folderName,string initFolder = null)
		{
			var initialPath = initFolder ?? Directory.GetCurrentDirectory();
			var currentPath = initialPath;
			while (currentPath != Directory.GetDirectoryRoot(initialPath))
			{
				foreach (var directory in Directory.GetDirectories(currentPath))
				{
					if (directory.EndsWith(folderName))
						return directory;
				}

				currentPath = Directory.GetParent(currentPath).FullName;
			}

			return string.Empty;
		}

		public bool IsVideoGood(byte[] bytes, Color profileColor, double threshHold, int frameSkip = 5)
		{
			if (bytes == null) return false;
			var simPath = IsVideoSimilar(profileColor, bytes, threshHold, frameSkip);
			return !string.IsNullOrEmpty(simPath);
		}

		public byte[] GenerateVideoThumbnail(byte[] video, int specificFrame = 5)
		{
			Helper.CreateDirectoryIfDoesNotExist(_ffmpegWrapper.TempVideoPath, _ffmpegWrapper.TempImagePath);

			var videoPath = string.Format(_ffmpegWrapper.TempVideoPath + "temp_video_{0}_{1}.mp4", Guid.NewGuid(), DateTime.UtcNow.Ticks);
			var imagePath = string.Format(_ffmpegWrapper.TempImagePath + "temp_image_{0}_{1}.jpeg", Guid.NewGuid(), DateTime.UtcNow.Ticks);

			File.WriteAllBytes(videoPath, video);
			try
			{
				GetVideoThumbnailFromProcess(videoPath, imagePath, specificFrame);
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
		public string IsVideoSimilar(Color profileColor, byte[] video, double threshHold, int frameSkip = 5)
		{
			var videoPath = string.Format(_ffmpegWrapper.TempVideoPath + "video_{0}.mp4", Guid.NewGuid());
			Helper.CreateDirectoryIfDoesNotExist(_ffmpegWrapper.TempVideoPath);

			var domColor = new Color();
			File.WriteAllBytes(videoPath, video);
			try
			{
				var outputFormat = $"{_ffmpegWrapper.TempImagePath}image-%04d_{Guid.NewGuid()}.jpeg";
				GetAllVideoThumbnailFromProcess(videoPath, outputFormat);
				var videoFrames = _ffmpegWrapper.TempImagePath.ReadImagesFromDirectory("*.jpeg").ToList();
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
		public void DisposeVideos(string loc = null, int retries = 8)
		{
			try
			{
				if (string.IsNullOrEmpty(loc))
				{
					Directory.EnumerateFiles(_ffmpegWrapper.TempImagePath, "*.jpeg").ToList()
						.ForEach(f => {
							var fileInfo = new FileInfo(f);
							for (var tries = 0; fileInfo.IsFileLocked() && tries < 8; tries++)
							{
								Thread.Sleep(1000);
							}
							fileInfo.Delete();
						});

					Directory.EnumerateFiles(_ffmpegWrapper.TempVideoPath, "*.mp4").ToList()
						.ForEach(f => {
							var fileInfo = new FileInfo(f);
							for (var tries = 0; fileInfo.IsFileLocked() && tries < retries; tries++)
							{
								Thread.Sleep(1000);
							}
							fileInfo.Delete();
						});
				}
				else
				{
					var fileInfo = new FileInfo(loc);
					for (var tries = 0; fileInfo.IsFileLocked() && tries < retries; tries++)
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
	}
}