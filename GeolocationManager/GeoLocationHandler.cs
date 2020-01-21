using System;
using GeolocationManager.Geonames;
using GeolocationManager.GoogleGeocode;
using GeolocationManager.IpInfo;
using GeolocationManager.Models;

namespace GeolocationManager
{
	public class GeoLocationHandler : IGeoLocationHandler
	{
		private readonly string _ipGeolocationToken;
		private readonly string _googleGeocodeToken;
		private readonly string _geonamesToken;
		public GeoLocationHandler(GeoLocationOptions options)
		{
			if (options == null)
				throw new Exception("Options cannot be null");

			_ipGeolocationToken = options.IpGeolocationToken;
			_googleGeocodeToken = options.GoogleGeocodeToken;
			_geonamesToken = options.GeonamesToken;
		}
		public IGeoNamesRequester GeoNames => new GeoNamesRequester(_geonamesToken);
		public IGoogleGeocodeRequester GoogleGeocode => new GoogleGeocodeRequester(_googleGeocodeToken);
		public IIpInfoRequester IpGeolocation => new IpInfoRequester(_ipGeolocationToken);
	}
}
