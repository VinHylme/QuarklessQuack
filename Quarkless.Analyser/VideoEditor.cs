using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.NET;
using Microsoft.Extensions.Options;
using Quarkless.Analyser.Extensions;
using Quarkless.Analyser.Models;

namespace Quarkless.Analyser
{
	public class VideoEditor : IVideoEditor
	{
		private readonly string _tempVideoPath;
		private readonly string _tempImagePath;
		private readonly string _ffmpegEnginePath;
		private readonly string _assemblyName;
		public VideoEditor(IOptions<MediaAnalyserOptions> options)
		{
//			if (options.Value.Equals(null))
//				throw new NullReferenceException();
//			if (!options.Value.FfmpegEnginePath.Contains(','))
//			{
//				throw new DirectoryNotFoundException("Please include the assembly name as well, e.g. {SolutionName}.{Project Name}");
//
//			}
//
//			var splitPath = options.Value.FfmpegEnginePath.Split(',');
			//_assemblyName = splitPath[0];
			
			_ffmpegEnginePath = GetPathByFolderName("References", AppDomain.CurrentDomain.BaseDirectory) + "/ffmpeg.exe"; //splitPath[1];
			//CreateFfmpegPathIfDoesNotExist();
			_tempImagePath = options.Value.TempImagePath;
			_tempVideoPath = options.Value.TempVideoPath;
		}
		public string GetPathByFolderName( string folderName,string initFolder = null)
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

		private byte[] ExtractResource(string project, string filename)
		{
			var bundleAssembly = AppDomain.CurrentDomain.GetAssemblies()
				.First(x => x.FullName.Contains(project));
			var name = bundleAssembly.GetManifestResourceNames()
				.First(x => x.EndsWith(filename));
		
			using (Stream manifestResourceStream = bundleAssembly.GetManifestResourceStream(name))
			{
				if (manifestResourceStream == null) return null;
				var ba = new byte[manifestResourceStream.Length];
				manifestResourceStream.Read(ba, 0, ba.Length);
				return ba;
			}
		}
		private void CreateFfmpegPathIfDoesNotExist()
		{
			if (File.Exists(_ffmpegEnginePath))
			{
				return;
			}
			var bx = ExtractResource(_assemblyName, _ffmpegEnginePath);
			File.WriteAllBytes(_ffmpegEnginePath, bx);
		}
		public async Task<bool> IsVideoGood(byte[] bytes, Color profileColor, double threshHold, int frameSkip = 5)
		{
			if (bytes == null) return false;
			var simPath = await IsVideoSimilar(profileColor, bytes, threshHold, frameSkip);
			return !string.IsNullOrEmpty(simPath);
		}
		private void CreateDirectoryIfDoesNotExist(params string[] paths)
		{
			foreach (var path in paths)
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
			}
		}
		public async Task<byte[]> GenerateVideoThumbnail(byte[] video, int specificFrame = 5)
		{
			CreateDirectoryIfDoesNotExist(_tempVideoPath, _tempImagePath);

			var videoPath = string.Format(_tempVideoPath + "temp_video_{0}_{1}.mp4", Guid.NewGuid(), DateTime.UtcNow.Ticks);
			var imagePath = string.Format(_tempImagePath + "temp_image_{0}_{1}.jpeg", Guid.NewGuid(), DateTime.UtcNow.Ticks);

			File.WriteAllBytes(videoPath, video);
			try
			{
				var engine = new Engine(_ffmpegEnginePath);
				
				await engine.GetThumbnailAsync(
					new MediaFile(videoPath),
					new MediaFile(imagePath),
					new ConversionOptions
					{
						Seek = TimeSpan.FromSeconds(specificFrame)
					});

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
			CreateDirectoryIfDoesNotExist(videoPath);

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
				var videoFrames = _tempImagePath.ReadImagesFromDirectory("*.jpeg").ToList();
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
					Directory.EnumerateFiles(_tempImagePath, "*.jpeg").ToList()
						.ForEach(f => {
							var fileInfo = new FileInfo(f);
							for (var tries = 0; fileInfo.IsFileLocked() && tries < 8; tries++)
							{
								Thread.Sleep(1000);
							}
							fileInfo.Delete();
						});

					Directory.EnumerateFiles(_tempVideoPath, "*.mp4").ToList()
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