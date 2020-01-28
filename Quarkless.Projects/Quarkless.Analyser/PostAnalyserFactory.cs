using Quarkless.Analyser.Models;

namespace Quarkless.Analyser
{
	public class PostAnalyserFactory : IPostAnalyserFactory
	{
		public static IPostAnalyser CreateInstance(MediaAnalyserOptions options)
		{
			return new PostAnalyser(new MediaManipulation(new VideoEditor(new FfmpegWrapper(options))));
		}

		IPostAnalyser IPostAnalyserFactory.CreateInstance(MediaAnalyserOptions options)
		{
			return CreateInstance(options);
		}
	}
}