namespace Quarkless.Models.InstagramAccounts.Enums
{
	public enum AgentState
	{
		NotStarted  = 0,
		Running = 1,
		Stopped = 2,
		Sleeping = 3,
		DeepSleep = 4,
		Blocked = 5,
		Challenge = 6,
		AwaitingActionFromUser = 7,
		Working = 8,
		BadProxy = 9
	}
}