using QuarklessContexts.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;
namespace ContentOrganiser
{
	public static class ProfileMatcher
	{
		public static bool Process(this byte[] @data, ProfileModel profile)
		{
			var ms  = new MemoryStream(@data);
			var image = Bitmap.FromStream(ms);
			var bitmap = new Bitmap(image);
			var setts = profile.Theme;
			var pixels = GetPixels(bitmap).GroupBy(color => color)
				.OrderByDescending(grp=>grp.Count())
				.Select(grp=>grp.Key)
				.Take(4);

			List<System.Drawing.Color> profileColors = new List<System.Drawing.Color>();
			foreach(var c in setts.Colors)
			{
				profileColors.Add(System.Drawing.Color.FromArgb(c.Alpha,c.Red,c.Green,c.Blue));
			}

			for(int x = 0 ; x < profileColors.Count && x < pixels.Count(); x++)
			{
				if (profileColors[x].ToArgb() >= pixels.ElementAt(x).ToArgb())
				{
					continue;
				}
				return false;
			}
			return true;
		}
		public static IEnumerable<System.Drawing.Color> GetPixels(Bitmap bitmap)
		{
			for (int x = 0; x < bitmap.Width; x++)
			{
				for (int y = 0; y < bitmap.Height; y++)
				{
					System.Drawing.Color pixel = bitmap.GetPixel(x, y);
					yield return pixel;
				}
			}
		}
	}
}
