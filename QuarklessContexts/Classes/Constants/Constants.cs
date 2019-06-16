namespace QuarklessContexts.Classes.Constants
{
	public static class Constants
	{
		public const string IPCHECK_HREF = "https://api.proxyscrape.com/?request=displayproxies&proxytype=http&timeout=5000&anonymity=elite&ssl=yes";
		public const string BASE_INSTAGRAM_HREF = "https://i.instagram.com/";
		public const string FilePath = @"..\State\{0}\";
		public const string GOBAL_LOG_FILEPATH = @"..\logerros.txt";
		public static string UserFilePath(string Userfile)
		{
			return $@"{Userfile}\state.json";
		}
	}
}
