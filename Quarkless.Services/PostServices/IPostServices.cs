using System.Threading.Tasks;

namespace Quarkless.Services.PostServices
{
	public interface IPostServices
	{
		Task<bool> FetchMedias(string tagsearch, int limit);
		Task<bool> RetrieveImages(string topic);
	}
}