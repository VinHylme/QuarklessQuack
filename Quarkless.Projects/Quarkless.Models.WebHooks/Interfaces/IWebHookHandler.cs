namespace Quarkless.Models.WebHooks.Interfaces
{
	public interface IWebHookHandler
	{
		void Handler(string json);
	}
}
