using QuarklessLogic.Handlers.TranslateService;
using QuarklessContexts.Models.FakerModels;
using QuarklessLogic.Handlers.EmailService;
using QuarklessLogic.Handlers.HashtagBuilder;
using QuarklessLogic.Handlers.TextGeneration;

namespace QuarklessLogic.Handlers.Util
{
	public interface IUtilProviders
	{
		IHashtagGenerator HashtagGenerator { get; }
		ITextGenerator TextGenerator { get; }
		ITranslateService TranslateService { get; }
		IEmailService EmailService { get; }
		FakerModel GeneratePerson(string locale = "en", string emailProvider = null, bool? isMale = null);
	}
}
