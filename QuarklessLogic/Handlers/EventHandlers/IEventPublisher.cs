using System.Threading.Tasks;

namespace QuarklessLogic.Handlers.EventHandlers
{
	public interface IEventPublisher
	{
		void Publish<TEvent>(TEvent @event);
		Task PublishAsync<TEvent>(TEvent @event);
	}
}