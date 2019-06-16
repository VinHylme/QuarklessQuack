using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Models
{
	public class RegisterAccountModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Name { get; set;}
		public string Email { get; set;}
	}
}
