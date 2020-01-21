using System.Threading.Tasks;
using GeolocationManager.GoogleGeocode.Models;

namespace GeolocationManager.GoogleGeocode
{
	public interface IGoogleGeocodeRequester
	{
		Task<GeocodeResponse> GetGeoInformation(double lat, double lng);
		Task<GeocodeResponse> GetGeoInformation(string address);
	}
}
