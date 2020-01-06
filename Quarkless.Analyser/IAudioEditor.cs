namespace Quarkless.Analyser
{
	public interface IAudioEditor
	{
		byte[] ConvertMp3ToWav(byte[] mp3Audio, int audioChannel = 1, int sampleRate = 16000);
		byte[] ConvertMp3ToWav(string mp3AudioPath, int audioChannel = 1, int sampleRate = 16000);
	}
}
