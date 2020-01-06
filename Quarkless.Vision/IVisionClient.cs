using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using Google.Cloud.Vision.V1;

namespace Quarkless.Vision
{
	public interface IVisionClient
	{
		Task<List<SpeechRecognitionResult>> RecogniseAudio(string uri);
		Task<List<SpeechRecognitionResult>> RecogniseAudio(Stream stream);
		Task<List<SpeechRecognitionResult>> RecogniseAudio(byte[] audio);
		Task<IEnumerable<EntityAnnotation>> DetectText(params string[] imageUrls);
		Task<IEnumerable<EntityAnnotation>> DetectText(IEnumerable<byte[]> imageBytes);
		Task<IEnumerable<WebDetection>> DetectImageWebEntities(IEnumerable<byte[]> imageBytes);
		Task<IEnumerable<WebDetection>> DetectImageWebEntities(params string[] imageUrls);
		Task<IEnumerable<ImageProperties>> DetectImageProperties(params string[] imageUrls);
		Task<IEnumerable<ImageProperties>> DetectImageProperties(IEnumerable<byte[]> imageBytes);
		Task<IEnumerable<FaceAnnotation>> DetectFacesFromImage(byte[] imageBytes);
		IEnumerable<FaceAnnotation> DetectFacesFromImages(IEnumerable<byte[]> imageBytes);
		Task<IEnumerable<EntityAnnotation>> AnnotateImage(byte[] imageBytes);
		IEnumerable<EntityAnnotation> AnnotateImages(IEnumerable<byte[]> imageBytes);
		Task<IEnumerable<FaceAnnotation>> DetectFacesFromImage(string imageUrl);
		IEnumerable<FaceAnnotation> DetectFacesFromImages(IEnumerable<string> imageUrls);
		Task<IEnumerable<EntityAnnotation>> AnnotateImage(string imageUrl);
		IEnumerable<EntityAnnotation> AnnotateImages(IEnumerable<string> imageUrls);
	}
}