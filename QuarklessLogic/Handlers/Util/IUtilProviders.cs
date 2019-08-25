using QuarklessLogic.Handlers.TranslateService;
using QuarklessContexts.Models.FakerModels;
using QuarklessLogic.Handlers.EmailService;

namespace QuarklessLogic.Handlers.Util
{
	public interface IUtilProviders
	{
		ITranslateService TranslateService { get; }
		IEmailService EmailService { get; }
		FakerModel GeneratePerson(string locale = "en", string emailProvider = null, bool? isMale = null);
	}
}
