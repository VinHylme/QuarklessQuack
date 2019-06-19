using QuarklessContexts.Models.Timeline;
using System;

namespace QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic
{
	public interface ITimelineLogic
	{
		bool AddToTimeline(RestModel restBody, DateTimeOffset executeTime);
	}
}