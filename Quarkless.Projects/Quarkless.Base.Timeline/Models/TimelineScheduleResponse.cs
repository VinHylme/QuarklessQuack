namespace Quarkless.Base.Timeline.Models
{
	public class TimelineScheduleResponse<T>
	{
		public T RequestData { get; set; }
		public int NumberOfFails { get; set; }
		public bool IsSuccessful { get; set; } = false;
	}
}