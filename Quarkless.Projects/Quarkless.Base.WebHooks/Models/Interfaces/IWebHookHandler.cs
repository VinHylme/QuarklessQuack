namespace Quarkless.Base.WebHooks.Models.Interfaces
{
	public interface IWebHookHandler
	{
		void Handler(string json);
	}
}
