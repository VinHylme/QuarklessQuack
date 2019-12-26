using System.Threading.Tasks;

namespace QuarklessLogic.Handlers.EventHandlers
{
	public interface IEventSubscriber<in TEvent>
	{
		Task Handle(TEvent @event);
	}
}
