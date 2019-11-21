using QuarklessLogic.QueueLogic.Jobs.JobOptions;

namespace QuarklessLogic.QueueLogic.Jobs.JobTypes
{
	public interface IJob<in TJobOptions> where TJobOptions: IJobOptions
	{
		void Perform(TJobOptions jobOptions);
	}
}
