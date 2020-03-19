using PuppeteerSharp;

namespace Quarkless.Base.PuppeteerClient.Models.Interfaces
{
	public interface IPuppeteerClient
	{
		Browser GetBrowser();
		void Dispose();
	}
}