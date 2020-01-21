using System;

namespace Quarkless.Models.Services.Automation.Interfaces
{
	public interface IActionOptions
	{
		DateTimeOffset ExecutionTime { get; set; }
	}
}