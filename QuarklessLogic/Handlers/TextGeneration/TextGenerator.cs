using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using QuarklessContexts.Extensions;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.Topics;
using QuarklessLogic.Logic.TopicLookupLogic;
using QuarklessLogic.ServicesLogic.CorpusLogic;

namespace QuarklessLogic.Handlers.TextGeneration
{
	//TODO ADD AI LATER, FOR NOW JUST USING MARKOV ALGO

	public class TextGenerator : ITextGenerator
	{
		private const string EMOJI_LIST = @"Smileys:😀😁😂🤣😃😄😅😆😉😊😋😎😍😘🥰😗😙😚🙂🤗🤩🤔🤨😐😑😶🙄😏😮🤐😯😌😛😜😝🤤😒🤑😲😤😢😭😨😩🤯😬😰😱🥵🥶😳🤪😵😇🤠🤡🥳🥴🤫🧐🤓😈💀👻👽🤖💩😺😸😹😻😼😽🙀
People and Fantasy:👶👧🧒👦👩🧑👨👵🧓👴👲🧕🧔👨‍🦰👩‍🦰👨‍🦱👩‍🦱👨‍🦲👩‍🦲👨‍🦳👩‍🦳👩‍⚕️👨‍⚕️👩‍🌾👨‍🌾👩‍🍳👨‍🍳👩‍🎓👨‍🎓👩‍🎤👨‍🎤👩‍🏫👨‍🏫👩‍🏭👨‍🏭👩‍💻👨‍💻👩‍💼👨‍💼👩‍🔧👨‍🔧👩‍🔬👨‍🔬👩‍🎨👨‍🎨👩‍🚒👨‍🚒👩‍✈️👨‍✈️👩‍🚀👨‍🚀👩‍⚖️👨‍⚖️👰🤵👸🤴🤶🎅👼🤰🤱👫👭👬💑👩‍❤️‍👩👨‍❤️‍👨💏👩‍❤️‍💋‍👩👨‍❤️‍💋‍👨👪👨‍👩‍👧👨‍👩‍👧‍👦👨‍👩‍👦‍👦👨‍👩‍👧‍👧👩‍👩‍👦👩‍👩‍👧👩‍👩‍👧‍👦👩‍👩‍👦‍👦👩‍👩‍👧‍👧👨‍👨‍👦👨‍👨‍👧👨‍👨‍👧‍👦👨‍👨‍👦‍👦👨‍👨‍👧‍👧👩‍👦👩‍👧👩‍👧‍👦👩‍👦‍👦👩‍👧‍👧👨‍👦👨‍👧👨‍👧‍👦👨‍👦‍👦👨‍👧‍👧🤲👐🙌👏🤝👍👎👊✊🤛🤜🤞✌️🤟🤘👌👈👉👆👇☝️✋🤚🖐🖖👋🤙💪🦵🦶🖕✍️🙏💍💄💋👄👅👂👃👣👁👀🧠🦴🦷🗣👤👥
Clothing and Accessories:🧥👚👕👖👔👗👙👘👠👡👢👞👟🥾🥿🧦🧤🧣🎩🧢👒🎓⛑👑👝👛👜💼🎒👓🕶🥽🥼🌂🧵🧶
Animals & Nature:🐶🐱🐭🐹🐰🦊🦝🐻🐼🦘🦡🐨🐯🦁🐮🐷🐽🐸🐵🙈🙉🙊🐒🐔🐧🐦🐤🐣🐥🦆🦢🦅🦉🦚🦜🦇🐺🐗🐴🦄🐝🐛🦋🐌🐚🐞🐜🦗🕷🕸🦂🦟🦠🐢🐍🦎🦖🦕🐙🦑🦐🦀🐡🐠🐟🐬🐳🐋🦈🐊🐅🐆🦓🦍🐘🦏🦛🐪🐫🦙🦒🐃🐂🐄🐎🐖🐏🐑🐐🦌🐕🐩🐈🐓🦃🕊🐇🐁🐀🐿🦔🐾🐉🐲🌵🎄🌲🌳🌴🌱🌿☘️🍀🎍🎋🍃🍂🍁🍄🌾💐🌷🌹🥀🌺🌸🌼🌻🌞🌝🌛🌜🌚🌕🌖🌗🌘🌑🌒🌓🌔🌙🌎🌍🌏💫⭐️🌟✨⚡️☄️💥🔥🌪🌈☀️🌤⛅️🌥☁️🌦🌧⛈🌩🌨❄️☃️⛄️🌬💨💧💦☔️☂️🌊🌫
Food & Drink:🍏🍎🍐🍊🍋🍌🍉🍇🍓🍈🍒🍑🍍🥭🥥🥝🍅🍆🥑🥦🥒🥬🌶🌽🥕🥔🍠🥐🍞🥖🥨🥯🧀🥚🍳🥞🥓🥩🍗🍖🌭🍔🍟🍕🥪🥙🌮🌯🥗🥘🥫🍝🍜🍲🍛🍣🍱🥟🍤🍙🍚🍘🍥🥮🥠🍢🍡🍧🍨🍦🥧🍰🎂🍮🍭🍬🍫🍿🧂🍩🍪🌰🥜🍯🥛🍼☕️🍵🥤🍶🍺🍻🥂🍷🥃🍸🍹🍾🥄🍴🍽🥣🥡🥢
Activity and Sports:⚽️🏀🏈⚾️🥎🏐🏉🎾🥏🎱🏓🏸🥅🏒🏑🥍🏏⛳️🏹🎣🥊🥋🎽⛸🥌🛷🛹🎿⛷🏂🏆🥇🥈🥉🏅🎖🏵🎗🎫🎟🎪🎭🎨🎬🎤🎧🎼🎹🥁🎷🎺🎸🎻🎲🧩♟🎯🎳🎮🎰
Travel & Places:🚗🚕🚙🚌🚎🏎🚓🚑🚒🚐🚚🚛🚜🛴🚲🛵🏍🚨🚔🚍🚘🚖🚡🚠🚟🚃🚋🚞🚝🚄🚅🚈🚂🚆🚇🚊🚉✈️🛫🛬🛩💺🛰🚀🛸🚁🛶⛵️🚤🛥🛳⛴🚢⚓️⛽️🚧🚦🚥🚏🗺🗿🗽🗼🏰🏯🏟🎡🎢🎠⛲️⛱🏖🏝🏜🌋⛰🏔🗻🏕⛺️🏠🏡🏘🏚🏗🏭🏢🏬🏣🏤🏥🏦🏨🏪🏫🏩💒🏛⛪️🕌🕍🕋⛩🛤🛣🗾🎑🏞🌅🌄🌠🎇🎆🌇🌆🏙🌃🌌🌉🌁
Objects:⌚️📱📲💻⌨️🖥🖨🖱🖲🕹🗜💽💾💿📀📼📷📸📹🎥📽🎞📞☎️📟📠📺📻🎙🎚🎛⏱⏲⏰🕰⌛️⏳📡🔋🔌💡🔦🕯🗑🛢💸💵💴💶💷💰💳🧾💎⚖️🔧🔨⚒🛠⛏🔩⚙️⛓🔫💣🔪🗡⚔️🛡🚬⚰️⚱️🏺🧭🧱🔮🧿🧸📿💈⚗️🔭🧰🧲🧪🧫🧬🧯🔬🕳💊💉🌡🚽🚰🚿🛁🛀🛀🏻🛀🏼🛀🏽🛀🏾🛀🏿🧴🧵🧶🧷🧹🧺🧻🧼🧽🛎🔑🗝🚪🛋🛏🛌🖼🛍🧳🛒🎁🎈🎏🎀🎊🎉🧨🎎🏮🎐🧧✉️📩📨📧💌📥📤📦🏷📪📫📬📭📮📯📜📃📄📑📊📈📉🗒🗓📆📅📇🗃🗳🗄📋📁📂🗂🗞📰📓📔📒📕📗📘📙📚📖🔖🔗📎🖇📐📏📌📍✂️🖊🖋✒️🖌🖍📝✏️🔍🔎🔏🔐🔒🔓
Symbols:❤️🧡💛💚💙💜🖤💔❣️💕💞💓💗💖💘💝💟💯";

		private readonly ICommentCorpusLogic _commentCorpusLogic;
		private readonly IMediaCorpusLogic _mediaCorpusLogic;
		private readonly ITopicLookupLogic _topicLookup;

		private readonly Dictionary<EmojiType, string> _emojiDict;

		public TextGenerator(ICommentCorpusLogic commentCorpusLogic, IMediaCorpusLogic mediaCorpusLogic, 
			ITopicLookupLogic topicLookup)
		{
			_commentCorpusLogic = commentCorpusLogic;
			_mediaCorpusLogic = mediaCorpusLogic;
			_topicLookup = topicLookup;
			var arrayOfEmojies = EMOJI_LIST.Split("\n", StringSplitOptions.RemoveEmptyEntries);
			_emojiDict = new Dictionary<EmojiType, string>(arrayOfEmojies
				.Select(_=>
				{
					var splitText = _.Split(':');
					return new KeyValuePair<EmojiType, string>
						(splitText[0].GetValueFromDescription<EmojiType>(),splitText[1].Split("/r")[0]);
				}));
		}
		public string GenerateNRandomEmojies(EmojiType set, int iterationMax)
		{
			var results = string.Empty;
			var typeEmoji = _emojiDict[set];

			if (iterationMax % 2 != 0) iterationMax += 1;
			var iterationsMax = SecureRandom.Next(1, iterationMax);

			var x = SecureRandom.Next(0, typeEmoji.Length - iterationsMax);
			if (x % 2 != 0) x += 1;

			var current = 0;
			for (; x < typeEmoji.Length; x += 2, current++)
			{
				if (current > iterationsMax) break;
				results += typeEmoji.Substring(x, 2);
			}

			return results;
		}

		public async Task<string> GenerateText(CTopic mediaTopic, int length, 
			EmojiType fallbackOnFail = EmojiType.Smileys)
		{
			var results = string.Empty;
			var tree = await _topicLookup.GetHighestParents(mediaTopic);

			if (!tree.Any())
				GenerateNRandomEmojies(fallbackOnFail, SecureRandom.Next(2, 4));

			var topicHash = tree.ComputeTopicHashCode();

			return results;
		}

		#region MARKOV TEXT GENERATION 
	
		public async Task<string> GenerateCaptionByMarkovChain(CTopic mediaTopic, int limit, 
			EmojiType fallback = EmojiType.Smileys)
		{
			if (mediaTopic == null)
				return string.Empty;
			if (limit <= 0)
				return string.Empty;

			const int searchLimit = 1500;
			const int takeSize = 15;

			var topicTree = await _topicLookup.GetHighestParents(mediaTopic);
			if (!topicTree.Any()) return GenerateNRandomEmojies(fallback, 6);
			var topicHash = topicTree.ComputeTopicHashCode();
			var medias = (await _mediaCorpusLogic.GetMedias(topicHash, limit:searchLimit, skip: false))
				.DistinctBy(_=>_.Caption)
				.Shuffle()
				.Take(takeSize)
				.ToList();

			if (!medias.Any())
			{
				return GenerateNRandomEmojies(fallback, 6);
			}

			var captions = medias.Select(sa =>
			{
				var pos = sa.Caption.LastIndexOf(',');
				return pos > 0 ? sa.Caption.Remove(pos, 1) : sa.Caption;
			});

			var dataMedia = Regex.Replace(string.Join(',', captions), @"\s+", " ").TrimEnd(' ');
			var dMediaDict = MarkovHelper.BuildTDict(dataMedia, takeSize / 2);
			return MarkovHelper.BuildString(dMediaDict, limit, true).TrimEnd(' ').Replace(",", " ");
		}
		public async Task<string> GenerateCommentByMarkovChain(CTopic mediaTopic, int limit,
			EmojiType fallback = EmojiType.Smileys)
		{
			if (mediaTopic == null)
				return string.Empty;
			if (limit <= 0)
				return string.Empty;

			const int searchLimit = 1500;
			const int takeSize = 15;

			var topicTree = await _topicLookup.GetHighestParents(mediaTopic);
			if (!topicTree.Any()) return GenerateNRandomEmojies(fallback, 6); ;
			var topicHash = topicTree.ComputeTopicHashCode();

			var commentCorpusResults = (await _commentCorpusLogic.GetComments(topicHash, limit: searchLimit, skip: false))
				.DistinctBy(_ => _.Comment)
				.Shuffle()
				.Take(takeSize)
				.ToList();

			if (!commentCorpusResults.Any())
				return GenerateNRandomEmojies(fallback, 6);

			var comments = commentCorpusResults.Select(sa =>
			{
				var pos = sa.Comment.LastIndexOf(',');
				return pos > 0 ? sa.Comment.Remove(pos, 1) : sa.Comment;
			});

			var dataComment = Regex.Replace(string.Join(',', comments), @"\s+", " ").TrimEnd(' ');
			var dCommentDict = MarkovHelper.BuildTDict(dataComment, takeSize / 2);
			return MarkovHelper.BuildString(dCommentDict, limit, true).TrimEnd(' ').Replace(",", " ");
		}

		//		public string MarkovTextGenerator(string filePath, int type, string topic, string language, int size, int limit)
		//		{
		//			if (type < 0 && type > 2) throw new Exception("invalid type");
		//			var joinedPath = string.Format(filePath,
		//				type == 0 ? "_comments" : type == 1 ? "_captions" : type == 2 ? "_bios" : null);
		//
		//			var reader = File.ReadAllLines(joinedPath).Skip(1).Select(_=>_.Split(",")).
		//				Where(l=>l.Count(p=>!string.IsNullOrEmpty(p))==3).
		//				Select(v=>new { Text = v[0].Replace("\"", ""), Topic = v[1].Replace("\"", ""), Language = v[2].Replace("\"", "") })
		//				.Where(x=>!x.Text.Contains("@"));
		//
		//			if(!reader.Any()) return null;
		//			var s = Regex.Replace(string.Join(',', reader.Select(sa => sa.Text)), @"\s+", " ").TrimEnd(' ');
		//			var t = MarkovHelper.BuildTDict(s, size);
		//			return MarkovHelper.BuildString(t, limit, true).TrimEnd(' ');
		//		}

		//		public string MarkovTextGenerator(string filePath, int limit, int size, bool exact = false)
		//		{
		//			if (!File.Exists(filePath)) { Console.WriteLine("Input file doesn't exist"); return null; }
		//			var s = Regex.Replace(File.ReadAllText(filePath), @"\s+", " ").TrimEnd(' ');
		//			var t = MarkovHelper.BuildTDict(s, size);
		//			return MarkovHelper.BuildString(t, limit, exact).TrimEnd(' ');
		//		}

		#endregion
	}
}
