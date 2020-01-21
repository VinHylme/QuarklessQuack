using System.Threading.Tasks;
using Quarkless.Geolocation.Google.Models;

namespace Quarkless.Geolocation.Google
{
	public interface IGoogleGeocodeRequester
	{
		Task<GeocodeResponse> GetGeoInformation(double lat, double lng);
		Task<GeocodeResponse> GetGeoInformation(string address);
	}
}