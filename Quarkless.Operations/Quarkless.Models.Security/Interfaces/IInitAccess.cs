using Quarkless.Models.ClientSender;

namespace Quarkless.Models.Security.Interfaces
{
	internal interface IInitAccess
	{
		AvailableClient[] AvailableClients { get; }
	}
}