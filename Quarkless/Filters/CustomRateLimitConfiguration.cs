using System;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Quarkless.Filters
{
	public class CustomRateLimitConfiguration : RateLimitConfiguration
	{
		public CustomRateLimitConfiguration(IHttpContextAccessor httpContextAccessor, IOptions<IpRateLimitOptions> ipOptions, IOptions<ClientRateLimitOptions> clientOptions) 
			: base(httpContextAccessor, ipOptions, clientOptions)
		{
		}
		protected override void RegisterResolvers()
		{
			base.RegisterResolvers();
			var stringify = JsonConvert.SerializeObject(TimeSpan.FromDays(1));
			IpResolvers.Add(new CustomResolver(HttpContextAccessor, IpRateLimitOptions.RealIpHeader));
		}
	}

	public class CustomResolver : IIpResolveContributor
	{
		private readonly IHttpContextAccessor _accessor;
		private readonly string _header;
		public CustomResolver(IHttpContextAccessor httpContextAccessor, string header)
		{
			_accessor = httpContextAccessor;
			_header = header;
		}

		public string ResolveIp()
		{
			return "hello gag";
		}
	}
}
