using Newtonsoft.Json;
using Quarkless.Services.Factories;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public enum FollowActionType
	{
		Any = 0,
		FollowBasedOnLikers = 1,
		FollowBasedOnCommenters = 2,
		FollowBasedOnTopic = 3
	}
	public class FollowUserAction : IActionCommit
	{
		private readonly IContentManager _builder;
		private readonly ProfileModel _profile;
		private readonly DateTime _executeTime;
		public FollowUserAction(IContentManager builder, ProfileModel profile, DateTime executeTime)
		{
			_builder = builder;
			_profile = profile;
			_executeTime = executeTime;
		}
		private long FollowBasedOnLikers()
		{
			IEnumerable<UserResponse<MediaDetail>> searchMedias = (IEnumerable<UserResponse<MediaDetail>>)MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic();
			searchMedias = searchMedias.Where(_ => _.Object.IsFollowing != null && _.Object.IsFollowing != true);
			var users_ = _builder.SearchInstagramMediaLikers(searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).Object.MediaId);
			int index = 0;

			while (users_.Count>index)
			{
				var findNominatedUser = _builder.SearchInstagramFullUserDetail(users_.ElementAt(SecureRandom.Next(users_.Count)).UserId);
				if(findNominatedUser==null) return 0;
				double ratioff = (double)findNominatedUser.UserDetail.FollowerCount / findNominatedUser.UserDetail.FollowingCount;

				if (ratioff > 1.0 && findNominatedUser.UserDetail.MediaCount > 5)
				{
					return findNominatedUser.UserDetail.Pk;
				}
				Thread.Sleep(TimeSpan.FromSeconds(SecureRandom.Next(1, 4)));
				index++;
			}
			return 0;
		}
		private long FollowBasedOnTopic()
		{
			IEnumerable<UserResponse<MediaDetail>> searchMedias = (IEnumerable<UserResponse<MediaDetail>>)MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic();
			searchMedias = searchMedias.Where(_=>_.Object.IsFollowing!=null && _.Object.IsFollowing!=true);
			return searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).UserId;
		}
		private long FollowBasedOnCommenters()
		{
			IEnumerable<UserResponse<MediaDetail>> searchMedias = (IEnumerable<UserResponse<MediaDetail>>)MediaFetcherManager.Begin.Commit(FetchType.Media,_builder, _profile).FetchByTopic();
			searchMedias = searchMedias.Where(_ =>
			{
				if (_ == null) return false;
				int commentCount = 0;
				int.TryParse(_.Object?.CommentCount, out commentCount);
				return _.Object.LikesCount > 5 && commentCount > 0 && !_.Object.IsCommentsDisabled;
			});
			var users_ = _builder.SearchInstagramMediaCommenters( 
				searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).Object.MediaId,1);
			int index = 0;
			while (users_.Count > index)
			{
				var comment = users_.ElementAt(index).Object;
				if (comment.LikeCount > 1) 
				{
					var findNominatedUser = _builder.SearchInstagramFullUserDetail(users_.ElementAt(SecureRandom.Next(users_.Count)).UserId);
					double ratioff = (double)findNominatedUser.UserDetail.FollowerCount / findNominatedUser.UserDetail.FollowingCount;

					if (ratioff > 1.0 && findNominatedUser.UserDetail.MediaCount > 5)
					{
						return findNominatedUser.UserDetail.Pk;
					}
					Thread.Sleep(TimeSpan.FromSeconds(SecureRandom.Next(1, 4)));
				}
				index++;
			}
			return 0;
		}
		public void Operate()
		{
			var topicSearch = _builder.GetTopics(_profile.TopicList, 15).GetAwaiter().GetResult();
			var topicSelect = topicSearch.ElementAt(SecureRandom.Next(topicSearch.Count));

			List<string> pickedSubsTopics = topicSelect.SubTopics.TakeAny(1).ToList();
			pickedSubsTopics.Add(topicSelect.TopicName);
			var searchMedias = _builder.SearchMediaDetailInstagram(pickedSubsTopics, 1);
			long nominatedFollower = 0;

			List<Chance<FollowActionType>> followActionsChances = new List<Chance<FollowActionType>>
			{
				new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnCommenters, Probability = 0.50},
				new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnLikers, Probability = 0.30},
				new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnTopic, Probability = 0.20}
			};

			var selectedFollowAction = SecureRandom.ProbabilityRoll(followActionsChances);
			switch (selectedFollowAction)
			{
				case FollowActionType.FollowBasedOnCommenters:
					var filteredMediaSearch = searchMedias.Where(_ => _.Object.LikesCount > 5 && int.Parse(_.Object?.CommentCount) > 0);
					nominatedFollower = FollowBasedOnCommenters();
					break;
				case FollowActionType.FollowBasedOnLikers:
					nominatedFollower = FollowBasedOnLikers();
					break;
				case FollowActionType.FollowBasedOnTopic:
					nominatedFollower = FollowBasedOnTopic();
					break;
			}
			if(nominatedFollower==0) return ; 

			RestModel restModel =  new RestModel
			{
				BaseUrl = string.Format(UrlConstants.FollowUser, nominatedFollower),
				RequestType = RequestType.POST,
				JsonBody = null
			};
			_builder.AddToTimeline(restModel, _executeTime);
		}

		public void Operate<TActionType>(TActionType actionType = default(TActionType))
		{
			try { 
				Console.WriteLine("Follow Action Started");
				long nominatedFollower = 0;
				//todo add Location?
				FollowActionType followActionTypeSelected = FollowActionType.FollowBasedOnTopic;
				if ((FollowActionType) (object) actionType == FollowActionType.Any) { 
					List<Chance<FollowActionType>> followActionsChances = new List<Chance<FollowActionType>>
					{
						new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnCommenters, Probability = 0.30},
						new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnLikers, Probability = 0.55},
						new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnTopic, Probability = 0.15}
					};
					followActionTypeSelected = SecureRandom.ProbabilityRoll(followActionsChances);
				}
				else {
					followActionTypeSelected = (FollowActionType) (object) actionType;
				}
				switch (followActionTypeSelected)
				{
					case FollowActionType.FollowBasedOnCommenters:
						nominatedFollower = FollowBasedOnCommenters();
						break;
					case FollowActionType.FollowBasedOnLikers:
						nominatedFollower = FollowBasedOnLikers();
						break;
					case FollowActionType.FollowBasedOnTopic:
						nominatedFollower = FollowBasedOnTopic();
						break;
				}
				if (nominatedFollower == 0) return;

				RestModel restModel = new RestModel
				{
					BaseUrl = string.Format(UrlConstants.FollowUser, nominatedFollower),
					RequestType = RequestType.POST,
					JsonBody = null
				};
				_builder.AddToTimeline(restModel, _executeTime);
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return ;
			}
		}
	}
}
