using QuarklessLogic.Handlers.HeartUpdater.Models;

namespace QuarklessLogic.Handlers.HeartUpdater
{
	public interface IHeartUpdater
	{
		void AddToQueue(RequestUpdateModel requestUpdate);
	}
}