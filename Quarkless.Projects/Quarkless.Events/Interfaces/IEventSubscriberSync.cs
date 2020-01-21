namespace Quarkless.Events.Interfaces
{
	public interface IEventSubscriberSync<in TEvent>
	{
		void Handle(TEvent @event);
	}
}