namespace Quarkless.Models.Security.Interfaces
{
	public interface IServerNetwork
	{
		void StartAccepting();
		void Dispose();
	}
}