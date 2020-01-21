using System.Threading.Tasks;

namespace Quarkless.Events.Interfaces
{
	public interface IEventPublisher
	{
		void Publish<TEvent>(TEvent @event);
		Task PublishAsync<TEvent>(TEvent @event);
	}
}
