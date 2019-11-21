namespace Quarkless.Security.ServerListener
{
	public interface IServerNetwork
	{
		void StartAccepting();
		void Dispose();
	}
}