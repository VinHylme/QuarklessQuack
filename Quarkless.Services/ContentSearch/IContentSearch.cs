using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;

namespace Quarkless.Services.ContentSearch
{
	public interface IContentSearch
	{
		Media SearchViaGoogle(SearchImageModel searchImageQuery);
		Task<Media> SearchMediaInstagram(UserStore user, List<string> topics, InstaMediaType mediaType, int limit);
		Task<Media> SearchMediaUser(UserStore user, int limit = 1);
		Media SearchViaYandexBySimilarImages(List<GroupImagesAlike> imagesSimilarUrls, int limit);
	}
}