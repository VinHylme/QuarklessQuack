using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Models.Auth
{
	public class EmailConfirmationModel
	{
		public string Username { get; set; }
		public string ConfirmationCode { get; set; }
		public bool CreateAlias { get; set; }
	}
}
