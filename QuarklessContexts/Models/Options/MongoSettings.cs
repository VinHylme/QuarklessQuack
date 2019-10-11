namespace QuarklessContexts.Models.Options
{
	public class MongoSettings
	{
		public string ControlDatabase { get; set; }
		public string MainDatabase { get; set; }
		public string ConnectionString { get; set; }
		public string ContentDatabase { get; set; }
		public string SchedulerDatabase { get; set; }
	}
}
