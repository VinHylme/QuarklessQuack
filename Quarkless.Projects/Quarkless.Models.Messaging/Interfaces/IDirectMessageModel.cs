using System.Collections.Generic;

namespace Quarkless.Models.Messaging.Interfaces
{
	public interface IDirectMessageModel
	{
		IEnumerable<string> Recipients { get; set; }
	}
}