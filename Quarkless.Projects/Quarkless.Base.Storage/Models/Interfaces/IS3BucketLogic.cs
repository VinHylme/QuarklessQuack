using System.IO;
using System.Threading.Tasks;

namespace Quarkless.Base.Storage.Models.Interfaces
{
	public interface IS3BucketLogic : IStorage
	{
		Task<bool> CreateBucket(string bucketName);
		Task<bool> UploadFile(string filePath, string keyName, string bucketName = null);
		Task<bool> UploadStreamFile(string filePath, string keyName, string bucketName = null);
		Task<string> UploadStreamFile(Stream stream, string keyName, string bucketName = null);
	}
}