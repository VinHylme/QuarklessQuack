using System.Threading.Tasks;

namespace Quarkless.Services.DataFetcher
{
	public interface IFetcher
	{
		Task Begin(Settings settings);
	}
}