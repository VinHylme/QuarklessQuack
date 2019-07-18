using QuarklessContexts.Models.Proxies;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessLogic.Handlers.TranslateService
{
	public interface ITranslateService
	{
		IEnumerable<string> DetectLanguageYandex(params string[] texts);
		IEnumerable<string> DetectLanguageViaGoogle(bool selectMostOccuring = false, string splitPattern = "-", params string[] texts);
		IEnumerable<string> DetectLanguageRest(params string[] texts);
		void AddProxy(ProxyModel proxy);
	}
}