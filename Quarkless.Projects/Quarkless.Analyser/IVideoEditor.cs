using System.Drawing;
using System.Threading.Tasks;

namespace Quarkless.Analyser
{
	public interface IVideoEditor
	{
		bool IsVideoGood(byte[] bytes, Color profileColor, double threshHold, int frameSkip = 5);
		byte[] GenerateVideoThumbnail(byte[] video, int specificFrame = 5);
		string IsVideoSimilar(Color profileColor, byte[] video, double threshHold, int frameSkip = 5);
		void DisposeVideos(string loc = null, int retries = 8);
	}
}