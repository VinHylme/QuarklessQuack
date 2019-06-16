using QuarklessContexts.Models.ServicesModels;
using QuarklessLogic.Logic.DiscoverLogic;
using QuarklessRepositories.Repository.ServicesRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Quarkless.Services.PostServices
{
	public class PostServices : IPostServices
	{
		private readonly IDiscoverLogic _discoverLogic;
		private readonly IPostServicesRepository _postServicesRepository;
		public PostServices(IDiscoverLogic discoverLogic, IPostServicesRepository postServices)
		{
			_discoverLogic = discoverLogic;
			_postServicesRepository = postServices;
		}

		private void DownloadFile(string url,string name,string topic)
		{
			if (string.IsNullOrEmpty(url)) { return ;}
			var directoryPath = $@"{Environment.CurrentDirectory}\images\{topic}\picked";
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}
			using (WebClient myWebClient = new WebClient())
			{
				var bytesdata = myWebClient.DownloadData(url);

				


				myWebClient.DownloadFile(url,directoryPath+$@"\{name}");
			}
		}

		public async Task<bool> RetrieveImages(string topic)
		{
			var res = await _postServicesRepository.RetrievePosts(topic);
			int index = 0;
			foreach(var medi in res)
			{
				if(medi.Media!=null)
				{ 
					string extension = medi.MediaType == 1 ? ".jpg" : ".mp4";
					DownloadFile(medi.Media.Uri,("im"+index++)+extension,medi.Topic); 
				}
			}
			return true;
		}


		public async Task<bool> FetchMedias(string tagsearch, int limit)
		{
			var res = await _discoverLogic.GetTopHashtagMediaList(tagsearch,limit);// .GetTagFeed(tagsearch, limit);
			if (res.Succeeded)
			{
				var medias = res.Value.Medias;
				List<PostServiceModel> posts = new List<PostServiceModel>();

				foreach(var media in medias)
				{
					PostServiceModel post = new PostServiceModel();
					post.Topic = tagsearch;
					post.CanViewerReshare = media.CanViewerReshare;
					post.CreatedDate = media.TakenAt;
					post.LikeCount = media.LikesCount;
					post.MediaId = media.Pk;
					post.ViewsCount = media.ViewCount;
					post.TotalCommentCount = media.CommentsCount;
					post.UsedBeforeCount = 0;
					if(media.Location!=null)
						post.Location = new Location { LocationId = media.Location.Pk, Name = media.Location.Name };

					post.MediaType = (int) media.MediaType;

					post.UserTags = media.UserTags.Select(_ => 
					{ return new UserTag { User = _.User.Pk, PositionAt = new Position { X = _.Position.X, Y = _.Position.Y} };
					}).ToList();

					if(media.MediaType == InstagramApiSharp.Classes.Models.InstaMediaType.Image)
					{
						post.Media = media.Images.Select(_ => { return new Media { Height = _.Height, Uri = _.Uri, Width = _.Width};}).FirstOrDefault();
					}
					else if(media.MediaType == InstagramApiSharp.Classes.Models.InstaMediaType.Video)
					{
						post.Media = media.Videos.Select(_ => { return new Media { Height = _.Height, Uri = _.Uri, Width = _.Width }; }).FirstOrDefault();
					}
					else if(media.MediaType == InstagramApiSharp.Classes.Models.InstaMediaType.Carousel)
					{
						foreach(var carousel in media.Carousel)
						{
							PostServiceModel postInner = new PostServiceModel{
								Topic = post.Topic,
								CanViewerReshare = post.CanViewerReshare,
								CreatedDate = post.CreatedDate,
								LikeCount = post.LikeCount,
								Location = post.Location,
								MediaId = post.MediaId,
								MediaType = post.MediaType,
								TotalCommentCount = post.TotalCommentCount,
								UsedBeforeCount = post.UsedBeforeCount,
								UserTags = post.UserTags,
								ViewsCount = post.ViewsCount
							};
							if (carousel.MediaType == InstagramApiSharp.Classes.Models.InstaMediaType.Image)
							{
								postInner.Media = carousel.Images.Select(_ => { return new Media { Height = _.Height, Uri = _.Uri, Width = _.Width }; }).FirstOrDefault();
							}
							else if (carousel.MediaType == InstagramApiSharp.Classes.Models.InstaMediaType.Video)
							{
								postInner.Media = carousel.Videos.Select(_ => { return new Media { Height = _.Height, Uri = _.Uri, Width = _.Width }; }).FirstOrDefault();
							}
							
							posts.Add(postInner);
						}
					}
					posts.Add(post);
				}

				var bulkres = await _postServicesRepository.BulkAdd(posts);
				if (bulkres)
				{
					return true;
				}
				return false;
			}
			return false;
		}
	}
}
