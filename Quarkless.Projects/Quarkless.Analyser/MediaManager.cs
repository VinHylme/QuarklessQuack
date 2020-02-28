using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Quarkless.Analyser
{
	public class MediaManager : IMediaManager
	{
		public byte[] DownloadMedias(List<string> urls, int poz)
		{
			using var webClient = new WebClient();
			try
			{
				return poz < 0 ? null : webClient.DownloadData(urls.ElementAt(poz));
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				DownloadMedias(urls, poz--);
				return null;
			}
		}
		public byte[] DownloadMediaLocal(string url) => File.ReadAllBytes(url);
		public byte[] DownloadMedia(string url)
		{
			using var webClient = new WebClient();
			try
			{
				webClient.Proxy = null;
				//webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
				webClient.Headers.Add("User-Agent: Other");
				return webClient.DownloadData(url);
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}
	}
}