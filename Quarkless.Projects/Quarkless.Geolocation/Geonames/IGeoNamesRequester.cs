using System.Threading.Tasks;
using Quarkless.Geolocation.Geonames.Models;

namespace Quarkless.Geolocation.Geonames
{
	public interface IGeoNamesRequester
	{
		Task<NearbyResponse> SearchNearbyPlace(double lat, double lng, int cityCount = 10, int radius = 10);
	}
}