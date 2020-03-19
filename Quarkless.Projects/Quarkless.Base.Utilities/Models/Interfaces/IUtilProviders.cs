using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Base.HashtagGenerator.Models.Interfaces;
using Quarkless.Base.TextGenerator.Models.Interfaces;

namespace Quarkless.Base.Utilities.Models.Interfaces
{
	public interface IUtilProviders
	{
		IHashtagGenerator HashtagGenerator { get; }
		ITextGenerator TextGenerator { get; }
		//ITranslateService TranslateService { get; }
		//IEmailService EmailService { get; }
		ISearchProvider SearchProvider { get; }
		FakerModel GeneratePerson(string locale = "en", string emailProvider = null, bool? isMale = null);
	}
}
