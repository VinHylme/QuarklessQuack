using System;
using InstagramApiSharp.Classes.Models;
using Quarkless.Analyser;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Factory.Action_Options.StrategySettings;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Objects;
using Quarkless.Models.Storage.Interfaces;

namespace Quarkless.Models.Actions.Factory.Action_Options
{
	public class PostActionOptions : IActionOptions
	{
		public PostMediaActionType PostMediaActionType { get; set; }
		public int ImageFetchLimit { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(1500, 2000);
		public PostStrategySettings StrategySettings { get; set; } = new PostStrategySettings();
		public readonly IPostAnalyser PostAnalyser;
		public readonly IS3BucketLogic S3BucketLogic;
		
		public PostActionOptions(IPostAnalyser postAnalyser, IS3BucketLogic s3BucketLogic,
			int imageFetchLimit = 20, PostStrategySettings strategySettings = null, XRange? timeFrame = null,
			PostMediaActionType postMediaAction = PostMediaActionType.Any)
		{
			PostAnalyser = postAnalyser ?? throw new Exception("Please provide the post analyser");
			S3BucketLogic = s3BucketLogic ?? throw new Exception("Please provide the S3bucketLogic");

			ImageFetchLimit = imageFetchLimit;
			PostMediaActionType = postMediaAction;

			if (timeFrame.HasValue)
				TimeFrameSeconds = timeFrame.Value;

			if (strategySettings != null)
				StrategySettings = strategySettings;
		}
	}
}