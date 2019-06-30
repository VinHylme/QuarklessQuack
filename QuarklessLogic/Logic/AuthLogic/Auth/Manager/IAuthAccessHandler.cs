namespace QuarklessLogic.Logic.AuthLogic.Auth.Manager
{
	public interface IAuthAccessHandler
	{
		string GetHash(string username, string clientId);
	}
}