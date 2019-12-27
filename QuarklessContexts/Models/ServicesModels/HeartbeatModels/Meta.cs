using System.Collections.Generic;

namespace QuarklessContexts.Models.ServicesModels.HeartbeatModels
{
	public class By
	{
		public string User { get; set; }
		public int ActionType { get; set;}
	}
	public class Meta<T>
	{
		public T ObjectItem { get; set; }
		public List<By> SeenBy { get; set; } = new List<By>();

		public Meta(T objectItem)
		{
			this.ObjectItem = objectItem;
		}
	}
}
