using System.Threading.Tasks;

namespace Quarkless.Events.Interfaces
{
	public interface IEventSubscriber<in TEvent>
	{
		Task Handle(TEvent @event);
	}
}
