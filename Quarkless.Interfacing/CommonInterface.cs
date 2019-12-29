﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using QuarklessContexts.Objects;
using QuarklessRepositories.RedisRepository.LoggerStoring;

namespace Quarkless.Interfacing
{
	/// <summary>
	/// Trying out new thing, make this the base of everything, to make life easier with logging, and other things
	/// </summary>
	public abstract class CommonInterface
	{
		private readonly ILoggerStore _logger;
		private readonly SString _section;
		protected CommonInterface(ILoggerStore logger, string section)
		{
			_logger = logger;
			_section = section;
		}

		public List<T> EmptyList<T>() => new List<T>();
		public List<T> List<T>(IEnumerable<T> items) => items.ToList();
		public int Len(string @string) => @string.Length;
		public int Len(IEnumerable<object> @array) => @array.Count();
		public long Len(object @object)
		{
			using (var stream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, @object);
				return stream.Length;
			}
		}
		
		public void Print(object message) => Console.Write(message);
		public void PrintLn(object message) => Console.WriteLine(message);

		/// <summary>
		/// Basically encapsulated try catch with log
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="code"></param>
		/// <param name="ids"></param>
		/// <param name="function"></param>
		/// <param name="accountId"></param>
		/// <param name="instagramAccountId"></param>
		/// <returns></returns>
		public async Task<T> RunCodeWithLoggerExceptionAsync<T>(Func<Task<T>> code, SString function,
			SString accountId, SString instagramAccountId)
		{
			try
			{
				return await code();
			}
			catch (Exception io)
			{
				await _logger.Log(new LoggerModel
				{
					AccountId = accountId,
					InstagramAccountId = instagramAccountId,
					Date = DateTime.UtcNow,
					Id = Guid.NewGuid().ToString(),
					Message = io.Message,
					Section = _section,
					Function = function,
					SeverityLevel = SeverityLevel.Exception
				});
				return default(T);
			}
		}
		public async Task<T> RunCodeWithExceptionAsync<T>(Func<Task<T>> code, SString function, SString accountId, SString instagramAccountId)
		{
			try
			{
				return await code();
			}
			catch (Exception io)
			{
				await _logger.Log(new LoggerModel
				{
					AccountId = accountId,
					InstagramAccountId = instagramAccountId,
					Date = DateTime.UtcNow,
					Id = Guid.NewGuid().ToString(),
					Message = io.Message,
					Section = _section,
					Function = function,
					SeverityLevel = SeverityLevel.Exception
				});
				return default(T);
			}
		}
		public async Task RunCodeWithExceptionAsync(Func<Task> code, SString function, SString accountId, SString instagramAccountId = null)
		{
			try
			{
				await code();
			}
			catch (Exception io)
			{
				await _logger.Log(new LoggerModel
				{
					AccountId = accountId,
					InstagramAccountId = instagramAccountId,
					Date = DateTime.UtcNow,
					Id = Guid.NewGuid().ToString(),
					Message = io.Message,
					Section = _section,
					Function = function,
					SeverityLevel = SeverityLevel.Exception
				});
			}
		}
		public async Task Warn(string message, SString function, SString accountId, SString instagramAccountId)
		{
			await _logger.Log(new LoggerModel
			{
				AccountId = accountId,
				InstagramAccountId = instagramAccountId,
				Section = _section,
				Function = function,
				Date = DateTime.UtcNow,
				Id = Guid.NewGuid().ToString(),
				Message = message,
				SeverityLevel = SeverityLevel.Warning
			});
		}
		public async Task Inform(string message, SString function, SString accountId, SString instagramAccountId)
		{
			await _logger.Log(new LoggerModel
			{
				AccountId = accountId,
				InstagramAccountId = instagramAccountId,
				Section = _section,
				Function = function,
				Date = DateTime.UtcNow,
				Id = Guid.NewGuid().ToString(),
				Message = message,
				SeverityLevel = SeverityLevel.Info
			});
		}
		public async Task Expect(string message, SString function, SString accountId, SString instagramAccountId)
		{
			await _logger.Log(new LoggerModel
			{
				AccountId = accountId,
				InstagramAccountId = instagramAccountId,
				Section = _section,
				Function = function,
				Date = DateTime.UtcNow,
				Id = Guid.NewGuid().ToString(),
				Message = message,
				SeverityLevel = SeverityLevel.Exception
			});
		}
	}
}
