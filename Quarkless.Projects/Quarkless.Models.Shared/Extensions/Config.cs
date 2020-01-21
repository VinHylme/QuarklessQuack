using Microsoft.Extensions.Configuration;
using Quarkless.Models.Shared.Models;

namespace Quarkless.Models.Shared.Extensions
{
	public class Config
	{
		public EnvironmentsAccess Environments => new ConfigInit().GetEnvironmentsDetails();
		public IConfiguration Configuration => new ConfigInit().GetConfiguration();
	}
}
