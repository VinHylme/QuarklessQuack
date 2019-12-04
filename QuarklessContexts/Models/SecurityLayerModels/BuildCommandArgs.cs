using System;

namespace QuarklessContexts.Models.SecurityLayerModels
{
	[Serializable]
	public class BuildCommandArgs : ICommandArgs
	{
		public string CommandName { get; set; }
		public ServiceTypes[] ServiceTypes { get; set; }
	}
}
