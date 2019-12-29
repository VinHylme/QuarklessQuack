using System.Threading.Tasks;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.Topics;

namespace QuarklessLogic.Handlers.TextGeneration
{
	public interface ITextGenerator
	{
		string GenerateNRandomEmojies(EmojiType set, int iterationMax);
		Task<string> GenerateCaptionByMarkovChain(CTopic mediaTopic, int limit,
			EmojiType fallback = EmojiType.Smileys);
		Task<string> GenerateCommentByMarkovChain(CTopic mediaTopic, int limit,
			EmojiType fallback = EmojiType.Smileys);
		string MarkovTextGenerator(string filePath, int limit, int size, bool exact = false);
		string MarkovTextGenerator(string filePath, int type, string topic, string language, int size, int limit);
	}
}