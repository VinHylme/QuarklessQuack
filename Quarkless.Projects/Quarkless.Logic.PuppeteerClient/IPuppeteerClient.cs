using PuppeteerSharp;

namespace Quarkless.Logic.PuppeteerClient
{
	public interface IPuppeteerClient
	{
		Browser GetBrowser();
		void Dispose();
	}
}