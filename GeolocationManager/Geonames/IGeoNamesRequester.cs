using System.Threading.Tasks;
using GeolocationManager.Geonames.Models;

namespace GeolocationManager.Geonames
{
	public interface IGeoNamesRequester
	{
		Task<NearbyResponse> SearchNearbyPlace(double lat, double lng, int cityCount = 10, int radius = 10);
	}
}
