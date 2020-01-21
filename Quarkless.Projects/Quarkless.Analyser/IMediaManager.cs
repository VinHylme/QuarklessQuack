using System.Collections.Generic;

namespace Quarkless.Analyser
{
	public interface IMediaManager
	{
		byte[] DownloadMedias(List<string> urls, int poz);
		byte[] DownloadMediaLocal(string url);
		byte[] DownloadMedia(string url);
	}
}