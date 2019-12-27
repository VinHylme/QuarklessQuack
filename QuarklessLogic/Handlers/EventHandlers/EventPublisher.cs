using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace QuarklessLogic.Handlers.EventHandlers
{
	public class EventPublisher : IEventPublisher
	{
		private readonly IServiceScope _scope;
		public EventPublisher(IServiceScope scope)
		{
			_scope = scope;
		}

		public void Publish<TEvent>(TEvent @event)
		{
			using (_scope)
			{
				var handlers = _scope.ServiceProvider.GetServices<IEventSubscriberSync<TEvent>>();
				foreach (var eventSubscriber in handlers)
				{
					eventSubscriber.Handle(@event);
				}
			}
		}

		public async Task PublishAsync<TEvent>(TEvent @event)
		{
			using (_scope)
			{
				var handlers = _scope.ServiceProvider.GetServices<IEventSubscriber<TEvent>>();
				foreach (var eventSubscriber in handlers)
				{ 
					await eventSubscriber.Handle(@event);
				}
			}
		}
	}
}