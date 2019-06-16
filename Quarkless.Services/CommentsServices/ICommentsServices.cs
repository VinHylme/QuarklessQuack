using System.Threading.Tasks;

namespace Quarkless.Services.CommentsServices
{
	public interface ICommentsServices
	{
		Task<bool> FetchComments(string topic, int limit = 20);
	}
}