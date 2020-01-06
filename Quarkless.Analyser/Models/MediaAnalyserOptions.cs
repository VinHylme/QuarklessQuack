namespace Quarkless.Analyser.Models
{
	public class MediaAnalyserOptions
	{
		public string TempAudioPath { get; set; }
		public string TempVideoPath { get; set; }
		public string TempImagePath { get; set; }
		public string FfmpegEnginePath { get; set; }
		public bool IsOnWindows { get; set; }
	}
}
