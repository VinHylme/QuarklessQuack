using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Quarkless.MediaAnalyser
{
	public interface IPostAnalyser
	{
		Task<byte[]> GenerateVideoThumbnail(byte[] video, int specificFrame = 5);
		bool IsImageGood(byte[] imBytes, IEnumerable<Color> profileColors, double threshHold, Size size);
		Task<bool> IsVideoGood(byte[] bytes, Color profileColor, double threshHold, int frameSkip = 5);
		void DisposeVideos(string loc = null, int retries = 8);
	}
}