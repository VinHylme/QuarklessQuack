using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Models.ApiModels
{
	public class EditCollectionRequest
	{
		public string CollectionName { get; set; }
		public string PhotoCoverId { get; set; }
	}
}
