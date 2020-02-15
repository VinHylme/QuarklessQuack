using System.Threading.Tasks;
using Quarkless.Models.TextGenerator.Enums;
using Quarkless.Models.Topic;

namespace Quarkless.Models.TextGenerator.Interfaces
{
	public interface ITextGenerator
	{
		string GenerateSingleEmoji(EmojiType set);
		string GenerateNRandomEmojies(EmojiType set, int iterationMax);
		Task<string> GenerateCaptionByMarkovChain(CTopic mediaTopic, int limit,
			EmojiType fallback = EmojiType.Smileys);
		Task<string> GenerateCommentByMarkovChain(CTopic mediaTopic, int limit,
			EmojiType fallback = EmojiType.Smileys);
	}
}