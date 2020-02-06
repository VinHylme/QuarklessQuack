using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Events.Interfaces;

namespace Quarkless.Events
{
	public class EventPublisher : IEventPublisher
	{
		private readonly IContainer _container;
		public EventPublisher(IServiceCollection services)
		{
			var builder = new ContainerBuilder();
			builder.Populate(services);
			_container = builder.Build();
		}

		public void Publish<TEvent>(TEvent @event)
		{
			using var scope = _container.BeginLifetimeScope();
			var handlers = scope.Resolve<IEnumerable<IEventSubscriberSync<TEvent>>>();
			foreach (var eventSubscriber in handlers)
			{
				eventSubscriber.Handle(@event);
			}
			
		}

		public async Task PublishAsync<TEvent>(TEvent @event)
		{
			await using var scope = _container.BeginLifetimeScope();
			var handlers = scope.Resolve<IEnumerable<IEventSubscriber<TEvent>>>();
			foreach (var eventSubscriber in handlers)
			{
				await eventSubscriber.Handle(@event);
			}
		}
	}

}
