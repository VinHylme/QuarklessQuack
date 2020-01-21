using Quarkless.Models.Proxy;
using System.Collections.Generic;

namespace Quarkless.Models.TranslateService.Interfaces
{
	public interface ITranslateService
	{
		IEnumerable<string> DetectLanguage(params string[] texts);
		IEnumerable<string> DetectLanguageYandex(params string[] texts);
		IEnumerable<string> DetectLanguageViaGoogle(bool selectMostOccuring = false, string splitPattern = "-", params string[] texts);
		IEnumerable<string> DetectLanguageRest(params string[] texts);
		void AddProxy(ProxyModel proxy);
	}
}