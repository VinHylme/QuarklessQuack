using Microsoft.Extensions.Configuration;
using QuarklessContexts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Quarkless.Common
{
	public class Accessors
	{
		public AWSAccess AWSAccess { get; set; }
		public AWSPool AWSPool { get; set; }
		private readonly IConfiguration _configuration;
		public Accessors(IConfiguration configuration)
		{
			_configuration = configuration;
			AWSPool = new AWSPool()
			{
				AppClientID = configuration["AWSCredential:AppClientID"],
				AppClientSecret = configuration["AWSCredential:AppSecretKey"],
				AuthUrl = configuration["AWSCredential:AuthUrl"] + configuration["AWSCredential:PoolID"],
				PoolID = configuration["AWSCredential:PoolID"],
				Region = configuration["AWS:Region"]
			};
			AWSAccess = new AWSAccess
			{
				AccessKey = configuration["AWS:AccessKey"],
				Region = configuration["AWS:Region"],
				SecretKey = configuration["AWS:SecretKey"]
			};
		}
		public string GoogleCredentials(string path)
		{
			return File.ReadAllText(path);
		}
		public string ImageSearchEndpoint
		{
			get
			{
				return _configuration["Endpoints:ImageSearchEndpointGoogle"];
			}
		}
		public string ConnectionString
		{
			get
			{ 
				return _configuration["ConnectionStrings:MongoClientStrings"]; 
			}
		}
		public string MainDatabase
		{
			get
			{
				return _configuration["ConnectionStrings:DatabaseNames:Accounts"];
			}
		}
		public string ControlDatabase
		{
			get
			{
				return _configuration["ConnectionStrings:DatabaseNames:Control"];
			}
		}
		public string ContentDatabase
		{
			get
			{
				return _configuration["ConnectionStrings:DatabaseNames:Content"];
			}
		}
	}
}
