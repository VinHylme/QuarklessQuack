namespace Quarkless.Models.Shared.Extensions
{
	public class ShutDownInstance
	{
		private static bool isShuttingDown { get; set; }
		public static void ShutDown()
		{
			isShuttingDown = true;
		}

		public static bool IsShutDown => isShuttingDown;
	}
}