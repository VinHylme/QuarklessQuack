using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Queue.Jobs.JobOptions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.SearchModels;

namespace Quarkless.Services.ContentBuilder.ContentSearch
{
	public interface IContentSearch
	{
		Media SearchViaGoogle(SearchImageModel searchImageQuery);
		Task<Media> SearchMediaInstagram(UserStore user, List<string> topics, InstaMediaType mediaType, int limit);
		Task<Media> SearchMediaUser(UserStore user, int limit = 1, params string[] instagramAccounts);
		Media SearchViaYandexBySimilarImages(List<GroupImagesAlike> imagesSimilarUrls, int limit);
	}
}