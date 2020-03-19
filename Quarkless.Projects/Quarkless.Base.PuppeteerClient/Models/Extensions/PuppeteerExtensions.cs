using System;
using System.Threading.Tasks;
using PuppeteerSharp;

namespace Quarkless.Base.PuppeteerClient.Models.Extensions
{
	public static class PuppeteerExtensions
	{
		public static async Task ScrollBy(this Page page, int amount = 2)
		{
			var current = 0;
			while (current < amount)
			{
				await page.Mouse.WheelAsync(0, 2500);
				await Task.Delay(TimeSpan.FromSeconds(.75));
				current++;
			}
		}
	}
}