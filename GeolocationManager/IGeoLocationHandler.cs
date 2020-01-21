using GeolocationManager.Geonames;
using GeolocationManager.GoogleGeocode;
using GeolocationManager.IpInfo;

namespace GeolocationManager
{
	public interface IGeoLocationHandler
	{
		IGeoNamesRequester GeoNames { get; }
		IGoogleGeocodeRequester GoogleGeocode { get; }
		IIpInfoRequester IpGeolocation { get; }
	}
}
