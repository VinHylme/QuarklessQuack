using System.Collections.Generic;

namespace Quarkless.Models.Common.Models
{
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