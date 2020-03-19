namespace Quarkless.Base.Timeline.Models.Interfaces.TaskScheduler
{
	public interface IJob<in TJobOptions> where TJobOptions: IJobOptions
	{
		void Perform(TJobOptions jobOptions);
	}
}
