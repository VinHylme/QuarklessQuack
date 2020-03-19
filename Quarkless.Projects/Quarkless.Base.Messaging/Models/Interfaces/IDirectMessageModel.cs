using System.Collections.Generic;
using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Base.Messaging.Models.Interfaces
{
	public interface IDirectMessageModel : IExec
	{
		IEnumerable<string> Recipients { get; set; }
	}
}