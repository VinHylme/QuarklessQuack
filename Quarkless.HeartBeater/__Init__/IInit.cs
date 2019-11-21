using System.Threading.Tasks;
using Quarkless.HeartBeater.Interfaces.Models;

namespace Quarkless.HeartBeater.__Init__
{
	public interface IInit
	{
		Task Endeavor(Settings settings);
		Task Populate(CorpusSettings settings);
	}
}
