using Quarkless.Models.Agent.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.Services.Heartbeat.Enums;

namespace Quarkless.Run.Services.Delegator
{
	internal class Entry
	{
		#region Declerations
		#region Constants
		private const string DOCKER_URL = "npipe://./pipe/docker_engine";
		private const string CLIENT_SECTION = "Client";

		private const string HEARTBEAT_IMAGE_NAME = "quarkless/quarkless.services.heartbeat:latest";
		private const string HEARTBEAT_CONTAINER_NAME = "quarkless.heartbeat.";

		private const string AUTOMATOR_IMAGE_NAME = "quarkless/quarkless.services.automator:latest";
		private const string AUTOMATOR_CONTAINER_NAME = "quarkless.automator.";

		private const string DATA_FETCHER_IMAGE_NAME = "quarkless/quarkless.services.datafetch:latest";
		private const string DATA_FETCHER_CONTAINER_NAME = "quarkless.datafetch.";

		private const string NETWORK_MODE = "localnet";
		#endregion
		private static IAgentLogic _agentLogic; 
		private static DockerClient Client => new DockerClientConfiguration(new Uri(DOCKER_URL)).CreateClient();
		#endregion

		#region Build Services
		#endregion

		#region Docker Api Stuff
		static void RunCommand(string command)
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
						Arguments = "/C "+command
					}
				};
				process.ErrorDataReceived += (o, e) =>
				{
					Console.WriteLine(e.Data);
				};
				process.OutputDataReceived += (o, e) =>
				{
					if (e.Data.Contains("Successfully tagged"))
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
				AttachStderr = true,
				AttachStdout = true
			});
			foreach (var container in await GetContainersByName(DATA_FETCHER_CONTAINER_NAME))
			{
				await Client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
				Console.WriteLine("Successfully started data fetch app with ID {0}", container.ID);
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
						$"OPERATION_TYPE={((int)type).ToString()}"
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

		/// <summary>
		/// This application requires the quarkless.security application to run on your local machine
		/// for that please go to the quarkless.security project and run the .exe in the bin folder
		/// (create a shortcut for ease)
		/// Create images for heartbeat and automator services
		/// Delete and Recreate the containers for each user
		/// </summary>
		/// <returns></returns>
		private static async Task CarryOutOperation()
		{
			//var services = InitialiseClientServices().BuildServiceProvider();
			//_agentLogic = services.GetService<IAgentLogic>();


			var images = await Client.Images.ListImagesAsync(new ImagesListParameters());

			if (!images.Any(image => image.RepoTags.Contains(HEARTBEAT_IMAGE_NAME)))
			{
				Console.WriteLine("Creating image for {0}", HEARTBEAT_IMAGE_NAME);
				try
				{
					var projPath = Directory.GetParent(Environment.CurrentDirectory.Split("bin")[0]).FullName;
					var solutionPath = Directory.GetParent(projPath);
					var heartBeatPath = solutionPath + @"\Quarkless.Services.Heartbeat";
					RunCommand($"cd {solutionPath} & docker build -t {HEARTBEAT_IMAGE_NAME} -f {heartBeatPath + @"\Dockerfile"} .");
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
					var projPath = Directory.GetParent(Environment.CurrentDirectory.Split("bin")[0]).FullName;
					var solutionPath = Directory.GetParent(projPath);
					var automatorPath = solutionPath + @"\Quarkless.Services";
					RunCommand($"cd {solutionPath} & docker build -t {AUTOMATOR_IMAGE_NAME} -f {automatorPath + @"\Dockerfile"} .");
				}
				catch (Exception err)
				{
					Console.WriteLine(err.Message);
				}
			}

			if (!images.Any(image => image.RepoTags.Contains(DATA_FETCHER_IMAGE_NAME)))
			{
				Console.WriteLine("Creating image for {0}", DATA_FETCHER_IMAGE_NAME);
				try
				{
					var projPath = Directory.GetParent(Environment.CurrentDirectory.Split("bin")[0]).FullName;
					var solutionPath = Directory.GetParent(projPath);
					var dataFetchPath = solutionPath + @"\Quarkless.Services.DataFetcher";
					RunCommand($"cd {solutionPath} & docker build -t {DATA_FETCHER_IMAGE_NAME} -f {dataFetchPath + @"\Dockerfile"} .");

				}
				catch (Exception err)
				{
					Console.WriteLine(err.Message);
				}
			}

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

			Console.WriteLine("Recreating new instances of containers");
			// recreate the heartbeat containers for users
			var customers = (await _agentLogic.GetAllAccounts(0))?.ToList();

			if (customers == null || !customers.Any()) return;

			var opsTypes = Enum.GetValues(typeof(ExtractOperationType)).Cast<ExtractOperationType>();
			foreach (var opsType in opsTypes)
			{
				await CreateAndRunHeartbeatContainers(customers, opsType);
			}
			Console.WriteLine("Finished Heartbeat containers, now starting automator");

			await CreateAndRunDataFetchContainer();

			await Task.Delay(TimeSpan.FromMinutes(3)); // wait around 2.5 minutes before starting automation (populate data first)
			await CreateAndRunAutomatorContainers(customers);
			Console.WriteLine("Finished creating containers for automator");
		}

		static async Task Main(string[] args)
		{
			await CarryOutOperation();
		}
	}
}
