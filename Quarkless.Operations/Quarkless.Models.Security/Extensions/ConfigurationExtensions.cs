using Microsoft.Extensions.Configuration;

namespace Quarkless.Models.Security.Extensions
{
	public static class ConfigurationExtensions
	{
		public static IConfigurationBuilder Unlock(this IConfigurationBuilder cnfBuilder, int pass)
		{
			return cnfBuilder;
		}
	}
}
