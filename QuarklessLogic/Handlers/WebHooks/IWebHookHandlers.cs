namespace QuarklessLogic.Handlers.WebHooks
{
	public interface IWebHookHandlers
	{
		void StripeHandler(string json);
	}
}