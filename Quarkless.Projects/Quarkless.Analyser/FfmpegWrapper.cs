using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Quarkless.Analyser.Models;

namespace Quarkless.Analyser
{
	public class FfmpegWrapper : IFfmpegWrapper
	{
		private readonly string _ffmpegEnginePath;
		private readonly bool _isWindowsOs;

		public string TempVideoPath { get; }
		public string TempAudioPath { get; }
		public string TempImagePath { get; }

		public FfmpegWrapper(MediaAnalyserOptions options)
		{
			TempImagePath = options.TempImagePath;
			TempVideoPath = options.TempVideoPath;
			TempAudioPath = options.TempAudioPath;

			_isWindowsOs = options.IsOnWindows;
			_ffmpegEnginePath = _isWindowsOs ? $"{options.FfmpegEnginePath}ffmpeg.exe"
				: $"{options.FfmpegEnginePath}ffmpeg";
		}

		private byte[] ExtractResource(string project, string filename)
		{
			var bundleAssembly = AppDomain.CurrentDomain.GetAssemblies()
				.First(x => x.FullName.Contains(project));
			var name = bundleAssembly.GetManifestResourceNames()
				.First(x => x.EndsWith(filename));

			using Stream manifestResourceStream = bundleAssembly.GetManifestResourceStream(name);
			if (manifestResourceStream == null) return null;
			var ba = new byte[manifestResourceStream.Length];
			manifestResourceStream.Read(ba, 0, ba.Length);
			return ba;
		}
		public string RunProcess(string parameters)
		{
			var result = string.Empty;
			var proc = new Process
			{
				StartInfo =
				{
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					RedirectStandardInput = true,
					FileName = _ffmpegEnginePath,
					Arguments = parameters
				},
			};
			proc.OutputDataReceived += (sender, args) =>
			{
				//Console.WriteLine(args.Data);
				result += args.Data;
			};
			proc.ErrorDataReceived += (sender, args) =>
			{
				//Console.WriteLine(args.Data); 
				result += args.Data;
			};

			proc.Start();
			proc.BeginOutputReadLine();
			proc.BeginErrorReadLine();
			proc.WaitForExit();

			return result;
		}
	}
}
