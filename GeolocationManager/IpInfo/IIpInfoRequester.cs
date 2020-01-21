using System.Threading.Tasks;
using GeolocationManager.IpInfo.Models;

namespace GeolocationManager.IpInfo
{
	public interface IIpInfoRequester
	{
		Task<IpGeolocation> GetUserLocationFromIp(string ip);
	}
}
