using InstagramApiSharp.Classes.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Worker.Actions
{
	public interface IWActions
	{
		Task<object> Operate();
		Task<bool> Operate(List<object> medias);
		Task<bool> Operate(object item);
	}
}