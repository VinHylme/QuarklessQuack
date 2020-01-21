using Quarkless.Geolocation.Geonames;
using Quarkless.Geolocation.Google;
using Quarkless.Geolocation.IpInfo;

namespace Quarkless.Geolocation
{
	public interface IGeoLocationHandler
	{
		IGeoNamesRequester GeoNames { get; }
		IGoogleGeocodeRequester GoogleGeocode { get; }
		IIpInfoRequester IpGeolocation { get; }
	}
}