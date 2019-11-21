using System;

namespace QuarklessContexts.Models.SecurityLayerModels
{
	[Serializable]
	public class InitCommandArgs : ICommandArgs
	{
		public string CommandName { get; set; }
		public AvailableClient Client { get; set; }
	}
}