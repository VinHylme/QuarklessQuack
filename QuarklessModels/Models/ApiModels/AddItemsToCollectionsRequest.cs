using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Models.ApiModels
{
	public class AddItemsToCollectionsRequest
	{
		public List<string> MediaIds { get; set; }
	}
}
