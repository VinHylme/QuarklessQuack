using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using QuarklessContexts.Models.Options;

namespace QuarklessLogic.Logic.StorageLogic
{
	public class S3BucketLogic : IS3BucketLogic
	{
		private readonly IAmazonS3 _client;
		private readonly string _bucketName;
		public S3BucketLogic(IOptions<S3Options> s3Options, IAmazonS3 client)
		{
			_bucketName = s3Options.Value.BucketName;
			_client = client;
		}
		public async Task<bool> UploadFile(string filePath, string keyName, string bucketName = null)
		{
			try
			{
				var fileTransferUtility = new TransferUtility(_client);
				await fileTransferUtility.UploadAsync(filePath, bucketName ?? _bucketName, keyName);
				return true;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}
		public async Task<bool> UploadStreamFile(string filePath, string keyName, string bucketName = null)
		{
			try
			{
				var fileTransferUtility = new TransferUtility(_client);
				using(var fileToUpload = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					await fileTransferUtility.UploadAsync(fileToUpload, bucketName ?? _bucketName, keyName);
				}
				return true;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return false;
			}
		}
		public async Task<string> ReadObjectDataAsync(string keyName, string bucketName)
		{
			var responseBody = "";
			var request = new GetObjectRequest
			{
				BucketName = bucketName,
				Key = keyName
			};
			try
			{
				using (var response = await _client.GetObjectAsync(request))
				using (var responseStream = response.ResponseStream)
				using (var reader = new StreamReader(responseStream))
				{
					var title = response.Metadata["x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
					var contentType = response.Headers["Content-Type"];
					Console.WriteLine("Object metadata, Title: {0}", title);
					Console.WriteLine("Content type: {0}", contentType);

					responseBody = reader.ReadToEnd(); // Now you process the response body.
					return responseBody;
				}
			}
			catch (AmazonS3Exception e)
			{
				Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
				return null;
			}
			catch (Exception e)
			{
				Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
				return null;
			}
		}
		public async Task<string> UploadStreamFile(Stream stream, string keyName, string bucketName = null)
		{
			try
			{
				var fileTransferUtility = new TransferUtility(_client);
				var uploadRequest = new TransferUtilityUploadRequest
				{
					InputStream = stream,
					BucketName = bucketName ?? _bucketName,
					Key = keyName,
					CannedACL = S3CannedACL.PublicRead
				};
				var expiryUrlRequest = new GetPreSignedUrlRequest()
				{
					BucketName = bucketName ?? _bucketName,
					Key = keyName,
					Expires = DateTime.Now.AddDays(26)
				};
				//string url_ = $"https://{bucketName}.s3.eu-west-2.amazonaws.com/{keyName}";
				var url = _client.GetPreSignedURL(expiryUrlRequest);
				using(var client = new WebClient())
				{
					try
					{
						var response = client.DownloadString(url);
					}
					catch(WebException ex)
					{
						var res = (HttpWebResponse)ex.Response;
						if(res.StatusCode == HttpStatusCode.NotFound)
							url = null;
					}
				}

				if (!string.IsNullOrEmpty(url)) return url;
				await fileTransferUtility.UploadAsync(uploadRequest); 
				url = _client.GetPreSignedURL(expiryUrlRequest);

				return url;
			}
			catch (Exception io)
			{
				Console.WriteLine(io);
				return null;
			}
		}
		public async Task<bool> CreateBucket(string bucketName)
		{
			try
			{
				if (await AmazonS3Util.DoesS3BucketExistV2Async(_client, bucketName) == false)
				{
					var putBucketRequest = new PutBucketRequest
					{
						BucketName = bucketName,
						UseClientRegion = true
					};

					var response = await _client.PutBucketAsync(putBucketRequest);
					if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
					{
						return true;
					}
				}
				return false;
			}
			catch (AmazonS3Exception s3)
			{
				Console.WriteLine(s3.Message);
				return false;
			}
			catch (Exception io)
			{
				Console.WriteLine(io.Message);
				return false;
			}
		}
	}
}
