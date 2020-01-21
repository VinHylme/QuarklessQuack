using System.Threading.Tasks;
using Quarkless.Geolocation.IpInfo.Models;

namespace Quarkless.Geolocation.IpInfo
{
	public interface IIpInfoRequester
	{
		Task<IpGeolocation> GetUserLocationFromIp(string ip);
	}
}