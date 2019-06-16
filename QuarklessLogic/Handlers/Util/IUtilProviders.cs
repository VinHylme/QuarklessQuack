using QuarklessLogic.Handlers.TranslateService;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessLogic.Handlers.Util
{
	public interface IUtilProviders
	{
		ITranslateService TranslateService { get; }
	}
}
