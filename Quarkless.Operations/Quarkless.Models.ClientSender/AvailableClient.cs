using System;

namespace Quarkless.Models.ClientSender
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