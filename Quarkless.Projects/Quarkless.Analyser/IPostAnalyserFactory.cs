using Quarkless.Analyser.Models;

namespace Quarkless.Analyser
{
	public interface IPostAnalyserFactory
	{
		IPostAnalyser CreateInstance(MediaAnalyserOptions options);
	}
}