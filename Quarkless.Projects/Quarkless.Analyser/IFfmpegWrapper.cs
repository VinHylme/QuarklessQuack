namespace Quarkless.Analyser
{
	public interface IFfmpegWrapper
	{
		string RunProcess(string parameters);
		string TempVideoPath { get; }
		string TempAudioPath { get; }
		string TempImagePath { get; }
	}
}