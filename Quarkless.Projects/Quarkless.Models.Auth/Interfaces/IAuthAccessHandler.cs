namespace Quarkless.Models.Auth.Interfaces
{
	public interface IAuthAccessHandler
	{
		string GetHash(string username, string clientId);
	}
}