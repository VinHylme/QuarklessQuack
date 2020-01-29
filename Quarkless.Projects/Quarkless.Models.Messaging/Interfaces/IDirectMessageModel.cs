using Quarkless.Models.Common.Interfaces;
using System.Collections.Generic;

namespace Quarkless.Models.Messaging.Interfaces
{
	public interface IDirectMessageModel : IExec
	{
		IEnumerable<string> Recipients { get; set; }
	}
}