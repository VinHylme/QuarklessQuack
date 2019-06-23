using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Services.Interfaces
{
	public interface IActionCommit
	{
		void Operate();
		void Operate<TActionType>(TActionType actionType = default(TActionType));
	}
}
