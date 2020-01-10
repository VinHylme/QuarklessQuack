using System;

namespace QuarklessContexts.Models.SecurityLayerModels
{
	[Serializable]
	public class ArgData : ICommandArgs
	{
		public AvailableClient Client { get; set; }
		public ServiceTypes[] Services { get; set; }
		public bool UseLocal { get; set; }
	}

}