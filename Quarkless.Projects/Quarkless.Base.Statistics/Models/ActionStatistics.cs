using System.Collections.Generic;

namespace Quarkless.Base.Statistics.Models
{
	public class ActionStatistics
	{
		#region All Time
		public int AllTimeActionsMade { get; set; }
		public int AllTimeFollowActionsMade { get; set; }
		public int AllTimeLikePostActionsMade { get; set; }
		public int AllTimeLikeCommentActionsMade { get; set; }
		public int AllTimeCreateCommentActionsMade { get; set; }
		public int AllTimeCreatePostActionsMade { get; set; }
		public int AllTimeWatchStoryActionsMade { get; set; }
		public int AllTimeReactStoryActionsMade { get; set; }
		#endregion

		public ActionMade LastActionMade { get; set; }
		public List<ActionMade> ActionsMadeOverPastDay { get; set; }
		public List<ActionMade> ActionsMadeOverPastHour { get; set; }
	}
}