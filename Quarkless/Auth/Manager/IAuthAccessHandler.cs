namespace Quarkless.Auth.Manager
{
	public interface IAuthAccessHandler
	{
		string GetHash(string username, string clientId);
	}
}