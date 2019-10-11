namespace QuarklessContexts.JobClass
{
	public interface IJob<in TJobOptions> where TJobOptions: IJobOptions
	{
		void Perform(TJobOptions jobOptions);
	}
}
