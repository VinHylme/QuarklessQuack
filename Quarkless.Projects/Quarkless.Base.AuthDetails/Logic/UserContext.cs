using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Quarkless.Base.Auth.Common.Models.Enums;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Models.Common.Extensions;

namespace Quarkless.Base.AuthDetails.Logic
{
	public class UserContext : IUserContext
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		public UserContext(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}
		public string CurrentUser
		{
			get
			{
				if (_httpContextAccessor.HttpContext?.User != null && (_httpContextAccessor.HttpContext?.User?.Identity != null || _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated))
				{
					return _httpContextAccessor.HttpContext.User.Claims?.Single(_ => _.Type == "cognito:username").Value;
				}
				return null;
			}
		}
		public AuthTypes UserRoleLevel
		{
			get
			{
				if (_httpContextAccessor.HttpContext != null && (_httpContextAccessor.HttpContext?.User?.Identity != null || _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated))
				{
					return _httpContextAccessor.HttpContext.User.Claims?.Single(_ => _.Type == "cognito:groups").Value.GetValueFromDescription<AuthTypes>() ?? AuthTypes.Expired;
				}

				return AuthTypes.Expired;
			}
		}

		public string FocusInstaAccount
		{
			get
			{
				if (_httpContextAccessor.HttpContext?.User != null && (_httpContextAccessor.HttpContext?.User?.Identity != null || _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated))
				{
					return _httpContextAccessor.HttpContext.Request.Headers["FocusInstaAccount"].FirstOrDefault();
				}

				return null;
			}
			set { }
		}

		public string UserIpAddress => GetRequestIp(true);
		private string GetRequestIp(bool tryUseXForwardHeader = true)
		{
			string ip = null;
			if (tryUseXForwardHeader)
				ip = GetHeaderValueAs<string>("X-Forwarded-For").SplitCsv().FirstOrDefault();

			if (ip.IsNullOrWhitespace() && _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress != null)
				ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

			if (ip.IsNullOrWhitespace())
				ip = GetHeaderValueAs<string>("REMOTE_ADDR");
			if (ip.IsNullOrWhitespace())
				throw new Exception("Unable to determine caller's IP.");

			return ip;
		}
		private T GetHeaderValueAs<T>(string headerName)
		{
			StringValues values;

			if (!(_httpContextAccessor.HttpContext?.Request?.Headers?.TryGetValue(headerName, out values) ?? false))
				return default(T);
			var rawValues = values.ToString();   // writes out as Csv when there are multiple.

			if (!rawValues.IsNullOrWhitespace())
				return (T)Convert.ChangeType(values.ToString(), typeof(T));
			return default(T);
		}
		public bool IsAdmin => !string.IsNullOrEmpty(CurrentUser) && UserRoleLevel == AuthTypes.Admin;
		public bool UserAccountExists => CurrentUser != null && FocusInstaAccount != null;
	}

	public static class LocalExtension
	{
		public static List<string> SplitCsv(this string csvList, bool nullOrWhitespaceInputReturnsNull = false)
		{
			if (string.IsNullOrWhiteSpace(csvList))
				return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

			return csvList
				.TrimEnd(',')
				.Split(',')
				.AsEnumerable<string>()
				.Select(s => s.Trim())
				.ToList();
		}

		public static bool IsNullOrWhitespace(this string s)
		{
			return string.IsNullOrWhiteSpace(s);
		}
	}
}