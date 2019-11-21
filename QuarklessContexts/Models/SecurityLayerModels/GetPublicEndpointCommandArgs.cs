using System;

namespace QuarklessContexts.Models.SecurityLayerModels
{
	[Serializable]
	public class GetPublicEndpointCommandArgs : ICommandArgs
	{
		public string CommandName { get; set; }
		public bool GetAll { get; set; } = true;
	}
}