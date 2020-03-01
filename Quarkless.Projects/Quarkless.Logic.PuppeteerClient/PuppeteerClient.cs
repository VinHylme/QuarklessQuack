using System;
using PuppeteerSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quarkless.Logic.PuppeteerClient
{
	public class PuppeteerClient : IPuppeteerClient, IDisposable
	{
		private readonly ConcurrentQueue<Browser> _browsers;
		public PuppeteerClient(int numberOfInstances = 10)
		{
			Initialise();
			_browsers = new ConcurrentQueue<Browser>();
			var creationTasks = new Task[numberOfInstances];

			for (var i = 0; i < numberOfInstances; i++)
			{
				creationTasks[i] = (Task.Run(() => {
					_browsers.Enqueue(CreateBrowser());
				}));
			}

			var totalTimed = Stopwatch.StartNew();
			Task.WaitAll(creationTasks);
			totalTimed.Stop();
			Console.WriteLine($"Total time took to create all browsers: {totalTimed.Elapsed.TotalSeconds}");
		}

		private void Initialise()
		{
			var processes = Process.GetProcesses();
			foreach (var chrome in processes.Where(_=>_.ProcessName.Equals("chrome")))
			{
				chrome.Kill();
			}
		}

		private Browser CreateBrowser()
		{
			var timed = Stopwatch.StartNew();
			var browser = Puppeteer.LaunchAsync(new LaunchOptions
			{
				Headless = false,
				Args = new[] { "--no-sandbox" },
				DefaultViewport = null
			}).Result;

			timed.Stop();
			Console.WriteLine($"Time took to create browser: {timed.Elapsed.TotalSeconds}");
			return browser;
		}

		public Browser GetBrowser()
		{
			while (_browsers.IsEmpty)
			{
				Console.WriteLine("Waiting for browser to be available...");
				Thread.Sleep(850);
			}

			_browsers.TryDequeue(out var browser);
			browser.Closed += Browser_Closed;
			return browser;
		}

		private void Browser_Closed(object sender, EventArgs e)
		{
			_browsers.Enqueue(CreateBrowser());
		}

		public void Dispose()
		{
			foreach (var browser in _browsers)
			{
				browser.CloseAsync().GetAwaiter().GetResult();
			}
		}
	}
}
