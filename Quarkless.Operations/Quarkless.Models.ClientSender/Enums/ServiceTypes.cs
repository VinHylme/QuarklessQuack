using System.ComponentModel;

namespace Quarkless.Models.ClientSender.Enums
{
	public enum ServiceTypes
	{
		[Description("AddHangFrameworkServices")]
		AddHangFrameworkServices,
		[Description("AddLogicServices")]
		AddLogics,
		[Description("AddAuthHandlers")]
		AddAuthHandlers,
		[Description("AddConfigurators")]
		AddConfigurators,
		[Description("AddRepositories")]
		AddRepositories,
		[Description("AddHandlers")]
		AddHandlers,
		[Description("AddContexts")]
		AddContexts,
		[Description("AddRequestLogging")]
		AddRequestLogging,
		[Description("AddEventServices")]
		AddEventServices
	}

}
