using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;

namespace Quarkless.Vision
{
	public class VisionClient : IVisionClient
	{
		private readonly ImageAnnotatorClient _client;
		public VisionClient(string credentialJson)
		{
			var credential = GoogleCredential.FromJson(credentialJson).CreateScoped(ImageAnnotatorClient.DefaultScopes);
			var channel = new Grpc.Core.Channel(ImageAnnotatorClient.DefaultEndpoint.ToString(), credential.ToChannelCredentials());
			_client = ImageAnnotatorClient.Create(channel);
		}

		public async Task<IEnumerable<ImageProperties>> DetectImageProperties(params string[] imageUrls)
		{
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
			return propertiesResults;
		}
		public async Task<IEnumerable<ImageProperties>> DetectImageProperties(IEnumerable<byte[]> imageBytes)
		{
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
			return propertiesResults;
		}
		public async Task<IEnumerable<FaceAnnotation>> DetectFacesFromImage(byte[] imageBytes)
		{
			try
			{
				var response = await _client.DetectFacesAsync(Image.FromBytes(imageBytes));
				return response.Count > 0 ? response : Enumerable.Empty<FaceAnnotation>();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<FaceAnnotation>();
			}
		}
		public IEnumerable<IEnumerable<FaceAnnotation>> DetectFacesFromImages(IEnumerable<byte[]> imageBytes)
		{
			return imageBytes.Select(imageByte => _client.DetectFaces(Image.FromBytes(imageByte)));
		}
		public async Task<IEnumerable<EntityAnnotation>> AnnotateImage(byte[] imageBytes)
		{
			try
			{
				var response = await _client.DetectLabelsAsync(Image.FromBytes(imageBytes));
				return response.Count > 0 ? response : Enumerable.Empty<EntityAnnotation>();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<EntityAnnotation>();
			}
		}
		public IEnumerable<IEnumerable<EntityAnnotation>> AnnotateImages(IEnumerable<byte[]> imageBytes)
		{
			return imageBytes.Select(image => _client.DetectLabels(Image.FromBytes(image)));
		}
		public async Task<IEnumerable<FaceAnnotation>> DetectFacesFromImage(string imageUrl)
		{
			try
			{
				var response = await _client.DetectFacesAsync(Image.FromUri(imageUrl));
				return response.Count > 0 ? response : Enumerable.Empty<FaceAnnotation>();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<FaceAnnotation>();
			}
		}
		public IEnumerable<IEnumerable<FaceAnnotation>> DetectFacesFromImages(IEnumerable<string> imageUrls)
		{
			return imageUrls.Select(imageUrl => _client.DetectFaces(Image.FromUri(imageUrl)));
		}
		public async Task<IEnumerable<EntityAnnotation>> AnnotateImage(string imageUrl)
		{
			try
			{
				var response = await _client.DetectLabelsAsync(Image.FromUri(imageUrl));
				return response.Count > 0 ? response : Enumerable.Empty<EntityAnnotation>();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return Enumerable.Empty<EntityAnnotation>();
			}
		}
		public IEnumerable<IEnumerable<EntityAnnotation>> AnnotateImages(IEnumerable<string> imageUrls)
		{
			return imageUrls.Select(image => _client.DetectLabels(Image.FromUri(image)));
		}
	}
}
