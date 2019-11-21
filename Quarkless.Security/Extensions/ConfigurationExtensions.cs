using Microsoft.Extensions.Configuration;

namespace Quarkless.Security.Extensions
{
	internal static class ConfigurationExtensions
	{
		internal static IConfigurationBuilder Unlock(this IConfigurationBuilder cnfBuilder, int pass)
		{

			return cnfBuilder;
		}
	}
}
