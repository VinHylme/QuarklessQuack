using Quarkless.Models.Agent.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.Services.Heartbeat.Enums;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.InstagramAccounts.Enums;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Run.Services.Delegator.Extensions;
using SharedConfig = Quarkless.Models.Shared.Extensions.Config;

namespace Quarkless.Run.Services.Delegator
{
	internal class Entry
	{

		#region Declerations

		#region Constants

		private const int HEART_BEAT_WORKER_TYPE = 1;
		private const int AUTOMATOR_WORKER_TYPE = 2;

		#region Docker Constants

		private const string DOCKER_URL = "npipe://./pipe/docker_engine";
		private const string HEARTBEAT_IMAGE_NAME = "quarkless/quarkless.services.heartbeat:latest";
		private const string HEARTBEAT_CONTAINER_NAME = "quarkless.heartbeat.";

		private const string AUTOMATOR_IMAGE_NAME = "quarkless/quarkless.services.automator:latest";
		private const string AUTOMATOR_CONTAINER_NAME = "quarkless.automator.";

		private const string DATA_FETCHER_IMAGE_NAME = "quarkless/quarkless.services.datafetch:latest";
		private const string DATA_FETCHER_CONTAINER_NAME = "quarkless.datafetch.";
		
		private const string NETWORK_MODE = "localnet";

		#endregion

		#region Localhost Build Constants

		private const string RELEASE_PATH = @"bin\Release\publish";
		private const string DOTNET_CORE_VERSION = "3.1";
		private static string _debugPath = @$"bin\Debug\netcoreapp{DOTNET_CORE_VERSION}";

		private const string OPERATIONS_FILE = @"Quarkless.Operations\";

		private const string HEARTBEAT_FILE_NAME = "Quarkless.Run.Services.Heartbeat";
		private const string HEARTBEAT_FILE_LOCATION =
			OPERATIONS_FILE + @"Quarkless.Services.Heartbeat.Console\" + HEARTBEAT_FILE_NAME;

		private const string AUTOMATOR_FILE_NAME = "Quarkless.Run.Services.Automation";
		private const string AUTOMATOR_FILE_LOCATION =
			OPERATIONS_FILE + @"Quarkless.Services.Automation.Console\" + AUTOMATOR_FILE_NAME;


		private const string DATA_FETCHER_FILE_NAME = "Quarkless.Run.Services.DataFetcher";
		private const string DATA_FETCHER_FILE_LOCATION =
			OPERATIONS_FILE + @"Quarkless.Services.DataFetcher.Console\" + DATA_FETCHER_FILE_NAME;

		#endregion
		#endregion

		private static IAgentLogic _agentLogic;
		private static IInstagramAccountLogic _instagramAccountLogic;
		private static DockerClient Client;

		private static bool _useDockerContainer;
		private static string HeartBeatServiceFilePath => SolutionPath + HEARTBEAT_FILE_LOCATION;
		private static string DataFetcherServiceFilePath => SolutionPath + DATA_FETCHER_FILE_LOCATION;
		private static string AutomationServicePath => SolutionPath + AUTOMATOR_FILE_LOCATION;
		private static string SolutionPath => new SharedConfig().SolutionPath;
		#endregion

		#region For Docker
		static void RunCommandFromCmd(string command)
		{
			try
			{
				var process = new Process()
				{
					StartInfo = new ProcessStartInfo
					{
						UseShellExecute = false,
						CreateNoWindow = true,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						RedirectStandardInput = true,
						FileName = "cmd.exe",
						Arguments = "/C "+ command
					}
				};
				process.ErrorDataReceived += (o, e) =>
				{
					Console.WriteLine(e.Data);
				};
				process.OutputDataReceived += (o, e) =>
				{
					if (e.Data!=null && e.Data.Contains("Successfully tagged"))
					{
						Task.Delay(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
						process.Close();
					}
					Console.WriteLine(e.Data);
				};
				process.Exited += (o, e) =>
				{

				};
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				//var oud = process.StandardError.ReadToEnd();
				//var dm = process.StandardOutput.ReadToEnd();
				process.WaitForExit((int)TimeSpan.FromMinutes(3.5).TotalMilliseconds);
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}

		#region Docker Api Stuff
		static async Task<IList<ContainerListResponse>> GetContainersByName(string name)
		{
			return await Client.Containers.ListContainersAsync(new ContainersListParameters
			{
				All = true,
				Filters = new Dictionary<string, IDictionary<string, bool>>
				{
					{ "name",
						new Dictionary<string, bool>
						{
							{"/"+name, true}
						}
					}
				}
			});
		}
		static async Task CreateAndRunDataFetchContainer()
		{
			await Client.Containers.CreateContainerAsync(new CreateContainerParameters
			{
				Image = DATA_FETCHER_IMAGE_NAME,
				Name = $"{DATA_FETCHER_CONTAINER_NAME}Instance",
				HostConfig = new HostConfig
				{
					NetworkMode = NETWORK_MODE,
					RestartPolicy = new RestartPolicy()
					{
						Name = RestartPolicyKind.Always
					}
				},
				Env = new List<string>
				{
					$"DOTNET_ENV_RELEASE={new SharedConfig().EnvironmentType.GetDescription()}",
					$"DOTNET_RUNNING_IN_CONTAINER={true}"
				},
				AttachStderr = true,
				AttachStdout = true
			});
			foreach (var container in await GetContainersByName(DATA_FETCHER_CONTAINER_NAME))
			{
				await Client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
				Console.WriteLine("Successfully started data fetch app with ID {0}", container.ID);
				await Task.Delay(TimeSpan.FromSeconds(1.5));
			}
		}
		static async Task CreateAndRunHeartbeatContainers(List<ShortInstagramAccountModel> customers, ExtractOperationType type)
		{
			foreach (var customer in customers)
			{
				await Client.Containers.CreateContainerAsync(new CreateContainerParameters
				{
					Image = HEARTBEAT_IMAGE_NAME,
					Name = $"{HEARTBEAT_CONTAINER_NAME}{type.ToString()}.{customer.AccountId}.{customer.Id}",
					HostConfig = new HostConfig
					{
						NetworkMode = NETWORK_MODE,
						RestartPolicy = new RestartPolicy()
						{
							Name = RestartPolicyKind.Always
						}
					},
					Env = new List<string>
					{
						$"USER_ID={customer.AccountId}",
						$"USER_INSTAGRAM_ACCOUNT={customer.Id}",
						$"OPERATION_TYPE={((int)type).ToString()}",
						$"DOTNET_ENV_RELEASE={new SharedConfig().EnvironmentType.GetDescription()}",
						$"DOTNET_RUNNING_IN_CONTAINER={true}",
						$"USER_WORKER_ACCOUNT_TYPE={HEART_BEAT_WORKER_TYPE}"
					},
					AttachStderr = true,
					AttachStdout = true
				});
				Console.WriteLine("Successfully created automator app for customer {0}", customer.Username);
			}

			//start the containers
			foreach (var container in await GetContainersByName(HEARTBEAT_CONTAINER_NAME))
			{
				await Client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
				Console.WriteLine("Successfully started heartbeat app with ID {0}", container.ID);
				await Task.Delay(TimeSpan.FromSeconds(1.5));
			}
		}
		static async Task CreateAndRunAutomatorContainers(List<ShortInstagramAccountModel> customers)
		{
			foreach (var customer in customers)
			{
				await Client.Containers.CreateContainerAsync(new CreateContainerParameters
				{
					Image = AUTOMATOR_IMAGE_NAME,
					Name = $"{AUTOMATOR_CONTAINER_NAME}{customer.AccountId}.{customer.Id}",
					HostConfig = new HostConfig
					{
						NetworkMode = NETWORK_MODE,
						PortBindings = new Dictionary<string, IList<PortBinding>>
						{
							{ "51242/tcp", 
								new List<PortBinding>
								{
									new PortBinding
									{
										HostPort = "51242"
									}
								}
							}
						},
						RestartPolicy = new RestartPolicy()
						{
							Name = RestartPolicyKind.Always
						}
					},
					
					NetworkingConfig = new NetworkingConfig()
					{
						EndpointsConfig = new Dictionary<string, EndpointSettings>
						{
							{ "localnet", 
								new EndpointSettings()
								{
									Aliases = new List<string>{ "quarkless.local.automator" }
								}
							}
						}
					},
					Env = new List<string>
					{
						$"USER_ID={customer.AccountId}",
						$"USER_INSTAGRAM_ACCOUNT={customer.Id}",
						$"DOTNET_ENV_RELEASE={new SharedConfig().EnvironmentType.GetDescription()}",
						$"DOTNET_RUNNING_IN_CONTAINER={true}",
						$"USER_WORKER_ACCOUNT_TYPE={AUTOMATOR_WORKER_TYPE}"
					},
					AttachStderr = true,
					AttachStdout = true
				});
				Console.WriteLine("Successfully created automator app for customer {0}", customer.Username);
			}

			//start the containers
			foreach (var container in await GetContainersByName(AUTOMATOR_CONTAINER_NAME))
			{
				await Client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
				Console.WriteLine("Successfully started automator app with ID {0}", container.ID);
			}
		}
		#endregion

		private static async Task CarryOutDockerOperation()
		{
			#region Build Images if they don't Exist
			var images = await Client.Images.ListImagesAsync(new ImagesListParameters());
			
			if (!images.Any(image => image.RepoTags.Contains(HEARTBEAT_IMAGE_NAME)))
			{
				Console.WriteLine("Creating image for {0}", HEARTBEAT_IMAGE_NAME);
				try
				{
					RunCommandFromCmd($"cd {SolutionPath} & docker build -t {HEARTBEAT_IMAGE_NAME} -f {HeartBeatServiceFilePath + @"\Dockerfile"} .");
				}
				catch (Exception err)
				{
					Console.WriteLine(err.Message);
				}
			}
			
			if (!images.Any(image => image.RepoTags.Contains(AUTOMATOR_IMAGE_NAME)))
			{
				Console.WriteLine("Creating image for {0}", AUTOMATOR_IMAGE_NAME);
				try
				{
					RunCommandFromCmd($"cd {SolutionPath} & docker build -t {AUTOMATOR_IMAGE_NAME} -f {AutomationServicePath + @"\Dockerfile"} .");
				}
				catch (Exception err)
				{
					Console.WriteLine(err.Message);
				}
			}

			/*
			if (!images.Any(image => image.RepoTags.Contains(DATA_FETCHER_IMAGE_NAME)))
			{
				Console.WriteLine("Creating image for {0}", DATA_FETCHER_IMAGE_NAME);
				try
				{
					RunCommandFromCmd($"cd {SolutionPath} & docker build -t {DATA_FETCHER_IMAGE_NAME} -f {DataFetcherServiceFilePath + @"\Dockerfile"} .");
				}
				catch (Exception err)
				{
					Console.WriteLine(err.Message);
				}
			}
			*/
			#endregion

			#region Clear Currently Running Containers
			Console.WriteLine("Clearing out current containers...");
			foreach (var container in await GetContainersByName(DATA_FETCHER_CONTAINER_NAME))
			{
				Console.WriteLine("Removed {0}", container.ID);
				await Client.Containers.RemoveContainerAsync(container.ID,
					new ContainerRemoveParameters
					{
						Force = true
					});
			}
			foreach (var container in await GetContainersByName(HEARTBEAT_CONTAINER_NAME))
			{
				Console.WriteLine("Removed {0}", container.ID);
				await Client.Containers.RemoveContainerAsync(container.ID,
					new ContainerRemoveParameters
					{
						Force = true
					});
			}
			foreach (var container in await GetContainersByName(AUTOMATOR_CONTAINER_NAME))
			{
				Console.WriteLine("Removed {0}", container.ID);
				await Client.Containers.RemoveContainerAsync(container.ID,
					new ContainerRemoveParameters()
					{
						Force = true
					});
			}
			#endregion

			//await CreateAndRunDataFetchContainer();

			Console.WriteLine("Recreating new instances of containers");

			// recreate the heartbeat containers for users
			var customers = (await _agentLogic.GetAllAccounts(0))?
				.Where(_=>_.AgentState != (int) AgentState.Stopped).ToList();

			if (customers == null || !customers.Any()) return;

			var opsTypes = Enum.GetValues(typeof(ExtractOperationType)).Cast<ExtractOperationType>();

			foreach (var opsType in opsTypes.Where(_=>_ != ExtractOperationType.None))
				await CreateAndRunHeartbeatContainers(customers, opsType);

			Console.WriteLine("Finished Heartbeat containers, now starting automator");

			await Task.Delay(TimeSpan.FromMinutes(3)); // wait around 2.5 minutes before starting automation (populate data first)
			await CreateAndRunAutomatorContainers(customers);
			Console.WriteLine("Finished creating containers for automator");
		}

		#endregion

		#region For Running Locally
		private static bool IsRunning(string name) => Processes(name).Length > 0;
		private static Process[] Processes(string name) => Process.GetProcessesByName(name);
		private static void RunProcess(string file, string accountId, string instagramId, 
			ExtractOperationType type = ExtractOperationType.None, string env = "local",
			int workerType = HEART_BEAT_WORKER_TYPE)
		{
			try
			{
				var process = new Process
				{
					StartInfo =
					{
						UseShellExecute = false,
						CreateNoWindow = false,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						RedirectStandardInput = true,
						FileName = file
					}
				};

				process.StartInfo.EnvironmentVariables.Add("DOTNET_RUNNING_IN_CONTAINER","false");

				if(process.StartInfo.EnvironmentVariables.ContainsKey("DOTNET_ENV_RELEASE"))
					process.StartInfo.EnvironmentVariables["DOTNET_ENV_RELEASE"] = env;
				else
					process.StartInfo.EnvironmentVariables.Add("DOTNET_ENV_RELEASE", env);
				
				process.StartInfo.EnvironmentVariables.Add("USER_ID", accountId);
				process.StartInfo.EnvironmentVariables.Add("USER_INSTAGRAM_ACCOUNT", instagramId);
				process.StartInfo.EnvironmentVariables.Add("USER_WORKER_ACCOUNT_TYPE", workerType.ToString());

				if (type!= ExtractOperationType.None)
					process.StartInfo.EnvironmentVariables.Add("OPERATION_TYPE", ((int)type).ToString());

				process.ErrorDataReceived += (o, e) =>
				{
					Console.WriteLine(e.Data);
				};
				process.OutputDataReceived += (o, e) =>
				{
					Console.WriteLine(e.Data);
				};
				process.Exited += (o, e) =>
				{

				};
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}

		}
		private static void RunProcess(string file, string env="local")
		{
			try
			{
				var process = new Process
				{
					StartInfo =
					{
						UseShellExecute = false,
						CreateNoWindow = false,
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						RedirectStandardInput = true,
						FileName = file
					}
				};

				process.StartInfo.EnvironmentVariables.Add("DOTNET_RUNNING_IN_CONTAINER", "false");

				if (process.StartInfo.EnvironmentVariables.ContainsKey("DOTNET_ENV_RELEASE"))
					process.StartInfo.EnvironmentVariables["DOTNET_ENV_RELEASE"] = env;
				else
					process.StartInfo.EnvironmentVariables.Add("DOTNET_ENV_RELEASE", env);

				process.ErrorDataReceived += (o, e) =>
				{
					Console.WriteLine(e.Data);
				};
				process.OutputDataReceived += (o, e) =>
				{
					Console.WriteLine(e.Data);
				};
				process.Exited += (o, e) =>
				{

				};
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit((int)TimeSpan.FromMinutes(3.5).TotalMilliseconds);
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}

		}
		/// <summary>
		/// Use this if you don't want to run on docker
		/// If you are developing on Production make sure that you have published the services files in the correct path
		/// </summary>
		/// <param name="useDebugFiles"></param>
		private static async Task CarryOutLocalOperation(bool useDebugFiles = true)
		{
			var env = new SharedConfig().EnvironmentType;

			#region Initialise File Paths
			var configurationSolutionPath = useDebugFiles ? _debugPath : RELEASE_PATH;
			var heartbeatExe = Path.Combine(HeartBeatServiceFilePath, @$"{configurationSolutionPath}\{HEARTBEAT_FILE_NAME}.exe");
			var automationExe = Path.Combine(AutomationServicePath, @$"{configurationSolutionPath}\{AUTOMATOR_FILE_NAME}.exe");
			var dataFetcherExe = Path.Combine(DataFetcherServiceFilePath, $"{RELEASE_PATH}/{DATA_FETCHER_FILE_NAME}.exe");
			#endregion

			#region If Processes are currently running then end them
			if (IsRunning(heartbeatExe))
			{
				foreach (var process in Processes(heartbeatExe))
				{
					process.Kill(true);
				}
			}

			if (IsRunning(dataFetcherExe))
			{
				foreach (var process in Processes(dataFetcherExe))
				{
					process.Kill(true);
				}
			}

			if (IsRunning(automationExe))
			{
				foreach (var process in Processes(automationExe))
				{
					process.Kill(true);
				}
			}
			#endregion

			var customers = (await _agentLogic.GetAllAccounts(0))?.ToList();
			if (customers == null || !customers.Any()) return;

			var opsTypes = Enum.GetValues(typeof(ExtractOperationType)).Cast<ExtractOperationType>();

			foreach (var operationType in opsTypes)
			{
				foreach (var customer in customers)
				{
					RunProcess(heartbeatExe, customer.AccountId, customer.Id, operationType,
						env.GetDescription(), HEART_BEAT_WORKER_TYPE);
					await Task.Delay(TimeSpan.FromSeconds(1));
				}
			}

			await Task.Delay(TimeSpan.FromSeconds(25));

			//RunProcess(dataFetcherExe);

			await Task.Delay(TimeSpan.FromSeconds(45));
			foreach (var customer in customers)
			{
				RunProcess(automationExe, customer.AccountId, customer.Id, ExtractOperationType.None,
					env.GetDescription(), AUTOMATOR_WORKER_TYPE);
			}
		}
		#endregion

		static async Task Main()
		{
			var runInDocker = Environment.GetEnvironmentVariable("APP_RUN_IN_DOCKER");
			if (string.IsNullOrEmpty(runInDocker))
				return;

			var services = new ServiceCollection();
			services.IncludeServices();

			using var scope = services.BuildServiceProvider().CreateScope();
			_agentLogic = scope.ServiceProvider.GetService<IAgentLogic>();
			_instagramAccountLogic = scope.ServiceProvider.GetService<IInstagramAccountLogic>();
			bool.TryParse(runInDocker, out var useDocker);
			_useDockerContainer = useDocker;

			await _instagramAccountLogic.UpdateAgentStates(AgentState.NotStarted, HEART_BEAT_WORKER_TYPE);
			await _instagramAccountLogic.UpdateAgentStates(AgentState.NotStarted, AUTOMATOR_WORKER_TYPE);

			if (_useDockerContainer)
				Client = new DockerClientConfiguration(new Uri(DOCKER_URL)).CreateClient();

			if (_useDockerContainer)
				await CarryOutDockerOperation();
			else
				await CarryOutLocalOperation(true);
		}
	}
}
