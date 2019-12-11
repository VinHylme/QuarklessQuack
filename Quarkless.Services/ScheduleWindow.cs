using System.Collections.Generic;
using QuarklessContexts.Models.Timeline;

namespace Quarkless.Services
{
	public class ScheduleWindow
	{
		public List<ResultBase<TimelineItem>> CreatePostActions;
		public List<ResultBase<TimelineItem>> CommentingActions;
		public List<ResultBase<TimelineItem>> LikeMediaActions;
		public List<ResultBase<TimelineItem>> LikeCommentActions;
		public List<ResultBase<TimelineItem>> FollowUserActions;
		public List<ResultBase<TimelineItem>> All;
		public ScheduleWindow()
		{
			All = new List<ResultBase<TimelineItem>>();
			CreatePostActions = new List<ResultBase<TimelineItem>>();
			LikeMediaActions = new List<ResultBase<TimelineItem>>();
			FollowUserActions = new List<ResultBase<TimelineItem>>();
			LikeCommentActions = new List<ResultBase<TimelineItem>>();
			CommentingActions = new List<ResultBase<TimelineItem>>();
		}
	}
}