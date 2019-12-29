﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Vision.V1;

namespace Quarkless.Vision
{
	public interface IVisionClient
	{
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