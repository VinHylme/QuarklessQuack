using System;

namespace QuarklessContexts.Models.SecurityLayerModels
{
	[Serializable]
	public class AvailableClient
	{
		public string Uuid { get; set; }
		public string Name { get; set; }
		public override int GetHashCode()
		{
			return this.Uuid.GetHashCode() * this.Name.GetHashCode();
		}
	}
}
