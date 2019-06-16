using QuarklessLogic.Handlers.TranslateService;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessLogic.Handlers.Util
{
	public class UtilProviders : IUtilProviders
	{
		private readonly ITranslateService _translateService;
		public UtilProviders(ITranslateService translateService)
		{
			_translateService = translateService;
		}
		public ITranslateService TranslateService
		{
			get
			{
				return _translateService;
			}
		}
	}
}
