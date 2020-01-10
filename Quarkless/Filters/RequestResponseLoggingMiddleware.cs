using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using QuarklessContexts.Models.APILogger;
using QuarklessRepositories.RedisRepository.APILogger;

namespace Quarkless.Filters
{
	public class RequestResponseLoggingMiddleware
	{
		#region Fields
		private int _concurrentRequestsCount;
		private readonly RequestDelegate _next;
		private readonly MaxConcurrentRequestsOptions _options;
		private readonly MaxConcurrentRequestsEnqueuer _enqueuer;
		private readonly IAPILogCache _apiLogCache;
		private readonly SecurityHeadersPolicy _policy;
		#endregion

		public RequestResponseLoggingMiddleware(RequestDelegate next, 
			SecurityHeadersPolicy securityHeadersPolicy, 
			IAPILogCache apiLogCache,
			IOptions<MaxConcurrentRequestsOptions> options)
		{
			_concurrentRequestsCount = 0;

			_next = next ?? throw new ArgumentNullException(nameof(next));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));

			if (_options.LimitExceededPolicy != MaxConcurrentRequestsLimitExceededPolicy.Drop)
			{
				_enqueuer = new MaxConcurrentRequestsEnqueuer(_options.MaxQueueLength, (MaxConcurrentRequestsEnqueuer.DropMode)_options.LimitExceededPolicy, _options.MaxTimeInQueue);
			}

			_apiLogCache = apiLogCache;
			_policy = securityHeadersPolicy;
		}

		public async Task Invoke(HttpContext httpContext)
		{
			#region Response Headers
			if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));		

			var httpResponse = httpContext.Response;

			if (httpResponse == null) throw new ArgumentNullException(nameof(httpResponse));

			var headers = httpResponse.Headers;

			foreach (var headerValuePair in _policy.SetHeaders)
			{
				headers[headerValuePair.Key] = headerValuePair.Value;
			}

			foreach (var header in _policy.RemoveHeaders)
			{
				headers.Remove(header);
			}
			#endregion
			async Task Execute()
			{
				var sw = Stopwatch.StartNew();
				try
				{
					var request = new RequestMeta
					{
						RequestContentType = httpContext.Request.ContentType,
						RequestMethod = httpContext.Request.Method,
						RequestTimestamp = DateTime.UtcNow,
						RequestUri = httpContext.Request.Path,
						//RequestBody = httpContext.Request.Body,
					};
					await _next(httpContext);
					var response = new ResponseMeta
					{
						ResponseContentType = httpContext.Response.ContentType,
						ResponseStatusCode = httpContext.Response?.StatusCode,
						//ResponseBody = httpContext.Response.Body,
						ResponseTimestamp = DateTime.UtcNow
					};

					sw.Stop();
					if (response.ResponseStatusCode == 409)
					{

					}
					var log = new ApiLogMetaData
					{
						User = new UserDetail
						{
							Ip = httpContext.Connection?.RemoteIpAddress.ToString(),
							Identity = httpContext.User?.Claims.Select(o=>new IdentityUser
							{
								//Subject = new Subject
								//{
								//	Name = o.Subject.Name,
								//	AuthType = o.Subject.AuthenticationType,
								//},
								Type = o.Type,
								//Issuer = o.Issuer,
								Value = o.Value,
								//Props = o.Properties,
								ValueType = o.ValueType
							})
						},
						RequestMetaData = request,
						ResponseMeta =  response,
						TotalTimeTaken = sw.Elapsed
					};
					await _apiLogCache.LogData(log);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}

			if (_options.ExcludePaths.Any(path => httpContext.Request.Path.Value.StartsWith(path)))
			{
				await Execute();
				return;
			}

			if (_options.Enabled && CheckLimitExceeded() && !(await TryWaitInQueueAsync(httpContext.RequestAborted)))
			{
				if (!httpContext.RequestAborted.IsCancellationRequested)
				{
					var responseFeature = httpContext.Features.Get<IHttpResponseFeature>();

					responseFeature.StatusCode = StatusCodes.Status503ServiceUnavailable;
					responseFeature.ReasonPhrase = "Concurrent request limit exceeded.";
				}
			}
			else
			{
				try
				{
					await Execute();
				}
				finally
				{
					if (await ShouldDecrementConcurrentRequestsCountAsync())
					{
						Interlocked.Decrement(ref _concurrentRequestsCount);
					}
				}
			}
		}

		private async Task<string> FormatRequest(HttpRequest request)
		{
			var body = request.Body;

			//This line allows us to set the reader for the request back at the beginning of its stream.
			request.EnableBuffering();
			//We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
			var buffer = new byte[Convert.ToInt32(request.ContentLength)];

			//...Then we copy the entire request stream into the new buffer.
			await request.Body.ReadAsync(buffer, 0, buffer.Length);

			//We convert the byte[] into a string using UTF8 encoding...
			var bodyAsText = Encoding.UTF8.GetString(buffer);

			//..and finally, assign the read body back to the request body, which is allowed because of EnableRewind()
			request.Body = body;

			return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
		}
		private async Task<string> FormatResponse(HttpResponse response)
		{
			//We need to read the response stream from the beginning...
			response.Body.Seek(0, SeekOrigin.Begin);

			//...and copy it into a string
			string text = await new StreamReader(response.Body).ReadToEndAsync();

			//We need to reset the reader for the response so that the client can read it.
			response.Body.Seek(0, SeekOrigin.Begin);

			//Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)
			return $"{response.StatusCode}: {text}";
		}
		private bool CheckLimitExceeded()
		{
			bool limitExceeded;

			if (_options.Limit == MaxConcurrentRequestsOptions.ConcurrentRequestsUnlimited)
			{
				limitExceeded = false;
			}
			else
			{
				int initialConcurrentRequestsCount, incrementedConcurrentRequestsCount;
				do
				{
					limitExceeded = true;

					initialConcurrentRequestsCount = _concurrentRequestsCount;
					if (initialConcurrentRequestsCount >= _options.Limit)
					{
						break;
					}

					limitExceeded = false;
					incrementedConcurrentRequestsCount = initialConcurrentRequestsCount + 1;
				}
				while (initialConcurrentRequestsCount != Interlocked.CompareExchange(ref _concurrentRequestsCount, incrementedConcurrentRequestsCount, initialConcurrentRequestsCount));
			}

			return limitExceeded;
		}
		private async Task<bool> TryWaitInQueueAsync(CancellationToken requestAbortedCancellationToken)
		{
			return (_enqueuer != null) && (await _enqueuer.EnqueueAsync(requestAbortedCancellationToken));
		}
		private async Task<bool> ShouldDecrementConcurrentRequestsCountAsync()
		{
			return (_options.Limit != MaxConcurrentRequestsOptions.ConcurrentRequestsUnlimited)
			       && ((_enqueuer == null) || !(await _enqueuer.DequeueAsync()));
		}
	}
}
