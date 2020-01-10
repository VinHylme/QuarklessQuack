using System;

namespace QuarklessContexts.Models.SecurityLayerModels
{
	[Serializable]
	public class GetPublicEndpointCommandArgs : ICommandArgs
	{
		public bool GetAll { get; set; } = true;
		public bool UseLocal { get; set; }
	}
}