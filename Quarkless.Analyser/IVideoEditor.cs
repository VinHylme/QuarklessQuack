using System.Drawing;
using System.Threading.Tasks;

namespace Quarkless.Analyser
{
	public interface IVideoEditor
	{
		Task<bool> IsVideoGood(byte[] bytes, Color profileColor, double threshHold, int frameSkip = 5);
		Task<byte[]> GenerateVideoThumbnail(byte[] video, int specificFrame = 5);
		Task<string> IsVideoSimilar(Color profileColor, byte[] video, double threshHold, int frameSkip = 5);
		void DisposeVideos(string loc = null, int retries = 8);
	}
}