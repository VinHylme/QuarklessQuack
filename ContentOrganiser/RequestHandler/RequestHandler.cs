using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ContentOrganiser.RequestHandler
{
	public class RequestHandler
	{
		public RequestHandler()
		{

		}

		public string GetRequest(string query)
		{
			using(WebClient client = new WebClient())
			{
				return client.DownloadString(new Uri($@"https://www.pexels.com/search/{query}"));
			}
		}
		public Image GetImageFromUrl(string url)
		{
			return Bitmap.FromStream(new MemoryStream(new WebClient().DownloadData(url)));
		}
		public void DownloadImage(string url, string savePath)
		{
			using(WebClient client = new WebClient())
			{
				client.DownloadFile(url,savePath);
			}
		}
	}
}
