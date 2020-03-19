using System.Threading.Tasks;
using Quarkless.Base.TextGenerator.Models.Enums;
using Quarkless.Models.Common.Models.Topic;

namespace Quarkless.Base.TextGenerator.Models.Interfaces
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