namespace Quarkless.Base.Auth.Models.Interfaces
{
	public interface IAuthAccessHandler
	{
		string GetHash(string username, string clientId);
	}
}