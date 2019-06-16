using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuarklessLogic.Handlers.HeartUpdater.Models
{
	public class RequestUpdateModel
	{
		public string CollectionName { get; set; }
		public string FieldName {get; set; }
		public object Value { get; set; }
	}
}
