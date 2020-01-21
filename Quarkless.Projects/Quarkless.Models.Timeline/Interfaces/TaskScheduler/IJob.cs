namespace Quarkless.Models.Timeline.Interfaces.TaskScheduler
{
	public interface IJob<in TJobOptions> where TJobOptions: IJobOptions
	{
		void Perform(TJobOptions jobOptions);
	}
}
