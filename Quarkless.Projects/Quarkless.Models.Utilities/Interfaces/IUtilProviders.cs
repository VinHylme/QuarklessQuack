using Quarkless.Base.ContentSearch;
using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Models.EmailService.Interfaces;
using Quarkless.Models.HashtagGenerator.Interfaces;
using Quarkless.Models.TextGenerator.Interfaces;
using Quarkless.Models.TranslateService.Interfaces;

namespace Quarkless.Models.Utilities.Interfaces
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
