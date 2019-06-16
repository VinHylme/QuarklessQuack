using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuarklessLogic.Handlers.TranslateService
{
	public interface ITranslateService
	{
		IEnumerable<string> DetectLanguageViaGoogle(bool selectMostOccuring = false, string splitPattern = "-", params string[] texts);
	}
}