using System;
using System.IO;
using Quarkless.Analyser.Extensions;

namespace Quarkless.Analyser
{
	public class AudioEditor : IAudioEditor
	{
		private readonly IFfmpegWrapper _ffmpegWrapper;
		public AudioEditor(IFfmpegWrapper ffmpegWrapper)
		{
			_ffmpegWrapper = ffmpegWrapper;
		}

		public byte[] ConvertMp3ToWav(byte[] mp3Audio, int audioChannel = 1, int sampleRate = 16000)
		{
			try
			{
				Helper.CreateDirectoryIfDoesNotExist(_ffmpegWrapper.TempAudioPath);

				var audioInputPath = string.Format(_ffmpegWrapper.TempAudioPath + "temp_mp3_audio_{0}_{1}.mp3", 
					Guid.NewGuid(), DateTime.UtcNow.Ticks);

				var audioOutputWavPath = string.Format(_ffmpegWrapper.TempAudioPath + "temp_wav_audio_{0}_{1}.wav",
					Guid.NewGuid(), DateTime.UtcNow.Ticks);

				File.WriteAllBytes(audioInputPath, mp3Audio);

				var res = _ffmpegWrapper.RunProcess($"-i {audioInputPath} -acodec pcm_s16le -ac {audioChannel} -ar {sampleRate} {audioOutputWavPath}");
				var wavBytes = File.ReadAllBytes(audioOutputWavPath);

				File.Delete(audioInputPath);
				File.Delete(audioOutputWavPath);

				return wavBytes;
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
		public byte[] ConvertMp3ToWav(string mp3AudioPath, int audioChannel = 1, int sampleRate = 16000)
		{
			try
			{
				Helper.CreateDirectoryIfDoesNotExist(_ffmpegWrapper.TempAudioPath);

				var audioOutputWavPath = string.Format(_ffmpegWrapper.TempAudioPath + "temp_wav_audio_{0}_{1}.wav",
					Guid.NewGuid(), DateTime.UtcNow.Ticks);

				_ffmpegWrapper.RunProcess($"-i {mp3AudioPath} -acodec pcm_s16le -ac 1 -ar 16000 {audioOutputWavPath}");
				var wavBytes = File.ReadAllBytes(audioOutputWavPath);

				File.Delete(audioOutputWavPath);

				return wavBytes;
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}

	}
}