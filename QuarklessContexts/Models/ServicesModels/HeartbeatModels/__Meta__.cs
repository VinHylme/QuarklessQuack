using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.HeartbeatModels
{
	public struct By
	{
		public string User { get; set; }
		public int ActionType { get; set;}
	}
	public struct __Meta__<T>
	{
		public T ObjectItem { get; set; }
		public List<By> SeenBy { get; set; }

		public __Meta__(T objectItem)
		{
			this.ObjectItem = objectItem;
			SeenBy = new List<By>();
		}
	}
}
