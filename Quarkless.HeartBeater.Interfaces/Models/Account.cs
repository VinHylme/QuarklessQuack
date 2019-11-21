namespace Quarkless.HeartBeater.Interfaces.Models
{
	public struct Account
	{
		public string Username { get; set; }
		public Account(string username)
		{
			this.Username = username;
		}
	}
}
