﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Base.Actions.Models;
using Quarkless.Base.Actions.Models.Factory.Action_Options;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.Actions.Models.Models;
using Quarkless.Base.ContentInfo.Models.Interfaces;
using Quarkless.Base.Heartbeat.Models;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Base.Media.Models;
using Quarkless.Common.Timeline.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.Actions.Logic.Action_Instances
{
	internal class AccountCheckerAction : BaseAction, IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private AccountCheckerActionOptions _actionOptions;
		internal AccountCheckerAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic, ILookupLogic lookupLogic) : base(lookupLogic, ActionType.MaintainAccount, userStoreDetails)
		{
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_user = userStoreDetails;
			//_actionOptions = new AccountCheckerActionOptions();
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			var results = new ResultCarrier<EventActionModel>();
			if (_actionOptions == null)
			{
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message = $"Account Check option is empty, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}"
				};
				return results;
			}

			try
			{
				var currentUsersMedia = (await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUserOwnProfile,
					ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
					InstagramId = _user.ShortInstagram.Id,
					AccountId = _user.AccountId
				})).ToList();

				//remove duplicates from user's gallery
				var usersMediaInBytes = new List<ContainMedia>();
				{
					foreach (var mediaData in currentUsersMedia)
					{
						if (mediaData == null) continue;
						var medias = mediaData.ObjectItem.Medias;

						foreach (var userMedia in medias)
						{
							foreach (var url in userMedia.MediaUrl)
							{
								usersMediaInBytes.Add(new ContainMedia
								{
									MediaData = _actionOptions.PostAnalyser.Manager.DownloadMedia(url),
									MediaId = userMedia.MediaId,
									MediaType = (int) userMedia.MediaType
								});
							}
						}
					}

					var images = usersMediaInBytes.Where(a => a.MediaType == (int) InstaMediaType.Image).ToList();

					var duplicates = _actionOptions.PostAnalyser.Manipulation.ImageEditor
						.DuplicateImages(images.Select(s => s.MediaData), 0.95).ToArray()
						.Where(_ => images.Any(s => s.MediaData.Equals(_.OriginalImageData)))
						.Select(s =>
						{
							var image = images.FirstOrDefault(_ => _.MediaData.Equals(s.OriginalImageData));
							return image;
						})
						.ToArray();

					if (!duplicates.Any())
					{
						results.IsSuccessful = false;
						results.Info = new ErrorResponse
						{
							Message = "No Images found"
						};
						return results;
					}

					var @event = new EventActionModel($"ActionChecker_{_actionOptions.StrategySettings.StrategyType.ToString()}")
					{
						ActionType = ActionType.MaintainAccount,
						User = new UserStore
						{
							AccountId = _user.AccountId,
							InstagramAccountUsername = _user.InstagramAccountUsername,
							InstagramAccountUser = _user.InstagramAccountUser
						}
					};

					var incr = 0;
					foreach (var duplicateImage in duplicates)
					{
						var item = new DeleteMediaModel
						{
							MediaId = duplicateImage.MediaId,
							MediaType = duplicateImage.MediaType
						};
						@event.DataObjects.Add(new EventBody(item, item.GetType(), executionTime.AddMinutes(_actionOptions.StrategySettings.OffsetPerAction.TotalMinutes * incr++)));
					}

					results.IsSuccessful = true;
					results.Results = @event;
					return results;
				}
			}
			catch (Exception err)
			{
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message = err.Message,
					Exception = err
				};
				return results;
			}
			finally
			{
				Console.WriteLine($"Account Check Action Ended: { _user.AccountId}, { _user.InstagramAccountUsername}, { _user.InstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as AccountCheckerActionOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}
	}
}
