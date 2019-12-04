using System;

namespace QuarklessContexts.Models.SecurityLayerModels
{
	public enum BuildType
	{
		Development,
		Production
	}
	[Serializable]
	public class BuildCommandArgs : ICommandArgs
	{
		public string CommandName { get; set; }
		public BuildType BuildType { get; set; }
		public ServiceTypes[] ServiceTypes { get; set; }
	}
}
