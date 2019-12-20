using System.Threading.Tasks;
using QuarklessContexts.Models.Topics;

namespace QuarklessLogic.Handlers.TextGeneration
{
	public interface ITextGenerator
	{
		Task<string> GenerateCaptionByMarkovChain(CTopic mediaTopic, int limit);
		Task<string> GenerateCommentByMarkovChain(CTopic mediaTopic, int limit);
		string MarkovTextGenerator(string filePath, int limit, int size, bool exact = false);
		string MarkovTextGenerator(string filePath, int type, string topic, string language, int size, int limit);
		Task<string> MarkovIt(int type, string topic, string language, int limit);
	}
}