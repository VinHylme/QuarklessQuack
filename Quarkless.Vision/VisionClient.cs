using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using QuarklessContexts.Extensions;
using QuarklessRepositories.RedisRepository.SearchCache;

namespace Quarkless.Vision
{
	public class VisionClient : IVisionClient
	{
		private readonly SpeechClient _speechClient;
		private readonly ImageAnnotatorClient _client;
		private readonly ISearchingCache _searchingCache;
		public VisionClient(string credentialJson, ISearchingCache cache)
		{
			var credential = GoogleCredential.FromJson(credentialJson).CreateScoped(ImageAnnotatorClient.DefaultScopes);
			var visionChannel = new Grpc.Core.Channel(ImageAnnotatorClient.DefaultEndpoint.ToString(), credential.ToChannelCredentials());
			var speechChannel = new Grpc.Core.Channel(SpeechClient.DefaultEndpoint.ToString(), credential.ToChannelCredentials());
			_client = ImageAnnotatorClient.Create(visionChannel);
			_speechClient = SpeechClient.Create(speechChannel);
			_searchingCache = cache;
		}

		#region Audio Client Stuff

		public async Task<List<SpeechRecognitionResult>> RecogniseAudio(byte[] audio)
		{
			try
			{
				var results = await _speechClient.RecognizeAsync(new RecognitionConfig
				{
					Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
					SampleRateHertz = 16000,
					LanguageCode = "en"
				}, RecognitionAudio.FromBytes(audio));

				return results.Results?.ToList() ?? new List<SpeechRecognitionResult>();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<SpeechRecognitionResult>();
			}
		}
		public async Task<List<SpeechRecognitionResult>> RecogniseAudio(string uri)
		{
			try
			{
				var results = await _speechClient.RecognizeAsync(new RecognitionConfig
				{
					Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
					SampleRateHertz = 16000,
					LanguageCode = "en"
				}, RecognitionAudio.FetchFromUri(uri));

				return results.Results?.ToList() ?? new List<SpeechRecognitionResult>();
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<SpeechRecognitionResult>();
			}
		}
		public async Task<List<SpeechRecognitionResult>> RecogniseAudio(Stream stream)
		{
			try
			{
				var results = await _speechClient.RecognizeAsync(new RecognitionConfig
				{
					Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
					SampleRateHertz = 16000,
					LanguageCode = "en"
				}, RecognitionAudio.FromStream(stream));

				return results.Results?.ToList() ?? new List<SpeechRecognitionResult>();
			}
			catch(Exception err)
			{
				Console.WriteLine(err.Message);
				return new List<SpeechRecognitionResult>();
			}
		}
		#endregion

		#region Vision Client Stuff
		public async Task<IEnumerable<EntityAnnotation>> DetectText(params string[] imageUrls)
		{
			var hashId = imageUrls.ComputeTotalHash().ToString();
			var cacheCheck = await _searchingCache.GetSearchData<EntityAnnotation>(hashId);

			if (cacheCheck.Any())
				return cacheCheck;

			var textResults = new List<EntityAnnotation>();
			foreach (var imageUrl in imageUrls)
			{
				try
				{
					var response = await _client.DetectTextAsync(Image.FromUri(imageUrl));
					textResults.AddRange(response);
				}
				catch(Exception err)
				{
					Console.WriteLine(err.Message);
				}
			}

			if (textResults.Any())
				await _searchingCache.AddSearchData(hashId, textResults);
			return textResults;
		}
		public async Task<IEnumerable<EntityAnnotation>> DetectText(IEnumerable<byte[]> imageBytes)
		{
			var hashId = imageBytes.ComputeTotalHash().ToString();
			var cacheCheck = await _searchingCache.GetSearchData<EntityAnnotation>(hashId);

			if (cacheCheck.Any())
				return cacheCheck;

			var textResults = new List<EntityAnnotation>();
			foreach (var image in imageBytes)
			{
				try
				{
					var response = await _client.DetectTextAsync(Image.FromBytes(image));
					textResults.AddRange(response);
				}
				catch (Exception err)
				{
					Console.WriteLine(err.Message);
				}
			}

			if (textResults.Any())
				await _searchingCache.AddSearchData(hashId, textResults);
			return textResults;
		}
		public async Task<IEnumerable<WebDetection>> DetectImageWebEntities(params string[] imageUrls)
		{
			var hashId = imageUrls.ComputeTotalHash().ToString();
			var cacheCheck = await _searchingCache.GetSearchData<WebDetection>(hashId);

			if (cacheCheck.Any())
				return cacheCheck;

			var webResults = new List<WebDetection>();
			foreach (var imageUrl in imageUrls)
			{
				try
				{
					var response = await _client.DetectWebInformationAsync(Image.FromUri(imageUrl));
					webResults.Add(response);
				}
				catch (Exception err)
				{
					Console.WriteLine(err.Message);
				}
			}

			if (webResults.Any())
				await _searchingCache.AddSearchData(hashId, webResults);

			return webResults;
		}
		public async Task<IEnumerable<WebDetection>> DetectImageWebEntities(IEnumerable<byte[]> imageBytes)
		{
			var hashId = imageBytes.ComputeTotalHash().ToString();
			var cacheCheck = await _searchingCache.GetSearchData<WebDetection>(hashId);

			if (cacheCheck.Any())
				return cacheCheck;

			var webResults = new List<WebDetection>();
			foreach (var imageByte in imageBytes)
			{
				try
				{
					var response = await _client.DetectWebInformationAsync(Image.FromBytes(imageByte));
					webResults.Add(response);
				}
				catch (Exception err)
				{
					Console.WriteLine(err.Message);
				}
			}
			if(webResults.Any())
				await _searchingCache.AddSearchData(hashId, webResults);
			return webResults;
		}
		public async Task<IEnumerable<ImageProperties>> DetectImageProperties(params string[] imageUrls)
		{
			var hashId = imageUrls.ComputeTotalHash().ToString();
			var cacheCheck = await _searchingCache.GetSearchData<ImageProperties>(hashId);

			if (cacheCheck.Any())
				return cacheCheck;

			var propertiesResults = new List<ImageProperties>();
			foreach (var image in imageUrls)
			{
				try
				{
					var response = await _client.DetectImagePropertiesAsync(Image.FromUri(image));
					propertiesResults.Add(response);
				}
				catch (Exception err)
				{
					Console.WriteLine(err);
				}
			}

			if (propertiesResults.Any())
				await _searchingCache.AddSearchData(hashId, propertiesResults);

			return propertiesResults;
		}
		public async Task<IEnumerable<ImageProperties>> DetectImageProperties(IEnumerable<byte[]> imageBytes)
		{
			var hashId = imageBytes.ComputeTotalHash().ToString();
			var cacheCheck = await _searchingCache.GetSearchData<ImageProperties>(hashId);

			if (cacheCheck.Any())
				return cacheCheck;

			var propertiesResults = new List<ImageProperties>();
			foreach (var image in imageBytes)
			{
				try
				{
					var response = await _client.DetectImagePropertiesAsync(Image.FromBytes(image));
					propertiesResults.Add(response);
				}
				catch (Exception err)
				{
					Console.WriteLine(err);
				}
			}

			if (propertiesResults.Any())
				await _searchingCache.AddSearchData(hashId, propertiesResults);

			return propertiesResults;
		}
		public async Task<IEnumerable<FaceAnnotation>> DetectFacesFromImage(byte[] imageBytes)
		{
			try
			{
				var hashId = imageBytes.ComputeHash().ToString();
				var cacheCheck = await _searchingCache.GetSearchData<FaceAnnotation>(hashId);

				if (cacheCheck.Any())
					return cacheCheck;

				var response = await _client.DetectFacesAsync(Image.FromBytes(imageBytes));
				if (response.Any())
					await _searchingCache.AddSearchData(hashId, response);
				return response;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<FaceAnnotation>();
			}
		}
		public IEnumerable<FaceAnnotation> DetectFacesFromImages(IEnumerable<byte[]> imageBytes)
		{
			var hashId = imageBytes.ComputeTotalHash().ToString();
			var cacheCheck = _searchingCache.GetSearchData<FaceAnnotation>(hashId).Result;

			if (cacheCheck.Any())
				return cacheCheck;
			
			var results = imageBytes.SelectMany(imageByte => _client.DetectFaces(Image.FromBytes(imageByte)));
			_searchingCache.AddSearchData(hashId, results).GetAwaiter().GetResult();

			return results;
		}
		public async Task<IEnumerable<EntityAnnotation>> AnnotateImage(byte[] imageBytes)
		{
			try
			{
				var hashId = imageBytes.ComputeHash().ToString();
				var cacheCheck = await _searchingCache.GetSearchData<EntityAnnotation>(hashId);

				if (cacheCheck.Any())
					return cacheCheck;
				var response = await _client.DetectLabelsAsync(Image.FromBytes(imageBytes));

				if (response.Any())
					await _searchingCache.AddSearchData(hashId, response);
				return response;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<EntityAnnotation>();
			}
		}
		public IEnumerable<EntityAnnotation> AnnotateImages(IEnumerable<byte[]> imageBytes)
		{
			var hashId = imageBytes.ComputeTotalHash().ToString();
			var cacheCheck = _searchingCache.GetSearchData<EntityAnnotation>(hashId).Result;

			if (cacheCheck.Any())
				return cacheCheck;

			var results = imageBytes.SelectMany(image => _client.DetectLabels(Image.FromBytes(image)));

			if(results.Any())
				_searchingCache.AddSearchData(hashId, results).GetAwaiter().GetResult();
			return results;
		}
		public async Task<IEnumerable<FaceAnnotation>> DetectFacesFromImage(string imageUrl)
		{
			try
			{
				var hashId = imageUrl.ToByteArray().ComputeHash().ToString();
				var cacheCheck = await _searchingCache.GetSearchData<FaceAnnotation>(hashId);

				if (cacheCheck.Any())
					return cacheCheck;

				var response = await _client.DetectFacesAsync(Image.FromUri(imageUrl));
				if (response.Any())
					await _searchingCache.AddSearchData(hashId, response);
				return response;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<FaceAnnotation>();
			}
		}
		public IEnumerable<FaceAnnotation> DetectFacesFromImages(IEnumerable<string> imageUrls)
		{
			var hashId = imageUrls.ComputeTotalHash().ToString();
			var cacheCheck = _searchingCache.GetSearchData<FaceAnnotation>(hashId).Result;

			if (cacheCheck.Any())
				return cacheCheck;

			var results = imageUrls.SelectMany(imageUrl => _client.DetectFaces(Image.FromUri(imageUrl)));

			if (results.Any())
				_searchingCache.AddSearchData(hashId, results).GetAwaiter().GetResult();
			
			return results;
		}
		public async Task<IEnumerable<EntityAnnotation>> AnnotateImage(string imageUrl)
		{
			try
			{
				var hashId = imageUrl.ToByteArray().ComputeHash().ToString();
				var cacheCheck = await _searchingCache.GetSearchData<EntityAnnotation>(hashId);

				if (cacheCheck.Any())
					return cacheCheck;

				var response = await _client.DetectLabelsAsync(Image.FromUri(imageUrl));
				if (response.Any())
					await _searchingCache.AddSearchData(hashId, response);
				return response;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<EntityAnnotation>();
			}
		}
		public IEnumerable<EntityAnnotation> AnnotateImages(IEnumerable<string> imageUrls)
		{
			try
			{
				var hashId = imageUrls.ComputeTotalHash().ToString();
				var cacheCheck = _searchingCache.GetSearchData<EntityAnnotation>(hashId).Result;

				if (cacheCheck.Any())
					return cacheCheck;
				var results = imageUrls.SelectMany(imageUrl => _client.DetectLabels(Image.FromUri(imageUrl)));

				if(results.Any())
					_searchingCache.AddSearchData(hashId, results).GetAwaiter().GetResult();
				return results;
			}
			catch (Exception err)
			{
				return Enumerable.Empty<EntityAnnotation>();
			}
		}
		#endregion
	}
}
