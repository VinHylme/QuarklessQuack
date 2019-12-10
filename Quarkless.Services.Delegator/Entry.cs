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
using MongoDB.Driver;
using Quarkless.Common.Clients;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.SecurityLayerModels;
using QuarklessLogic.ServicesLogic.AgentLogic;

namespace Quarkless.Services.Delegator
{
	class Entry
	{
		private const string CLIENT_SECTION = "Client";
		private const string SERVER_IP = "localhost";
		private const string heartbeatImageName = "quarkless/quarkless.services.heartbeat:latest";
		private const string heartbeatContainerName = "/quarkless.heartbeat.";
		private static IAgentLogic _agentLogic;
		private static DockerClient _client => new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();

		#region Build Services
		private static IConfiguration MakeConfigurationBuilder()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory().Split("bin")[0])
				.AddJsonFile("appconfigs.json").Build();
		}
		private static IServiceCollection InitialiseClientServices()
		{
			var cIn = new ClientRequester(SERVER_IP, false);
			if (!cIn.TryConnect().GetAwaiter().GetResult())
				throw new Exception("Invalid Client");

			var caller = MakeConfigurationBuilder().GetSection(CLIENT_SECTION).Get<AvailableClient>();

			var validate = cIn.Send(new InitCommandArgs
			{
				Client = caller,
				CommandName = "Validate Client"
			});
			if (!(bool)validate)
				throw new Exception("Could not validated");

			var services = (IServiceCollection)cIn.Send(new BuildCommandArgs()
			{
				CommandName = "Build Services",
				ServiceTypes = new[]
				{
					ServiceTypes.AddConfigurators,
					ServiceTypes.AddContexts,
					ServiceTypes.AddHandlers,
					ServiceTypes.AddLogics,
					ServiceTypes.AddRepositories
				}
			});
			return services;
		}
		private static Task WithExceptionLogAsync(Func<Task> actionAsync)
		{
			try
			{
				return actionAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message}");
				throw;
			}
		}
		#endregion

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
			return await _client.Containers.ListContainersAsync(new ContainersListParameters
			{
				All = true,
				Filters = new Dictionary<string, IDictionary<string, bool>>
				{
					{ "name",
						new Dictionary<string, bool>
						{
							{name, true}
						}
					}
				}
			});
		}

		static async Task CreateAndRunHeartbeatContainers(List<ShortInstagramAccountModel> customers, ExtractOperationType type)
		{
			var workers = new NextList<ShortInstagramAccountModel>(await _agentLogic.GetAllAccounts(1));
			if (!workers.Any()) return;

			foreach (var customer in customers.Where(user=>user.AgentState == (int) AgentState.Running))
			{
				var worker = workers.MoveNextRepeater();
				await _client.Containers.CreateContainerAsync(new CreateContainerParameters
				{
					Image = heartbeatImageName,
					Name = $"quarkless.heartbeat.{type.ToString()}.{customer.AccountId}.{customer.Id}.{worker.Id}",
					HostConfig = new HostConfig
					{
						NetworkMode = "localnet",
						RestartPolicy = new RestartPolicy()
						{
							Name = RestartPolicyKind.Always
						}
					},
					Env = new List<string>
					{
						$"USER_ID={customer.AccountId}",
						$"USER_INSTAGRAM_ACCOUNT={customer.Id}",
						$"WORKER_USER_ID={worker.AccountId}",
						$"WORKER_INSTAGRAM_ACCOUNT={worker.Id}",
						$"OPERATION_TYPE={((int)type).ToString()}"
					},
					AttachStderr = true,
					AttachStdout = true
				});
			}

			//start the containers
			foreach (var container in await GetContainersByName(heartbeatContainerName))
			{
				await _client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
			}
		}
        static async Task Main(string[] args)
        {
	        var services = InitialiseClientServices().BuildServiceProvider();
	        _agentLogic = services.GetService<IAgentLogic>();


			var images = await _client.Images.ListImagesAsync(new ImagesListParameters());
			
			if (!images.Any(image => image.RepoTags.Contains(heartbeatImageName)))
			{
				try
				{
					var projPath = Directory.GetParent(Environment.CurrentDirectory.Split("bin")[0]).FullName;
					var solutionPath = Directory.GetParent(projPath);
					var heartBeatPath = solutionPath + @"\Quarkless.Services.Heartbeat";
					RunCommand($"cd {solutionPath} & docker build -t {heartbeatImageName} -f {heartBeatPath + @"\Dockerfile"} .");
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}

			//get all heartbeat containers then remove them
			foreach (var container in await GetContainersByName(heartbeatContainerName))
			{
				await _client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters
				{
					Force = true
				});
			}

			// recreate the heartbeat containers for users
			var customers = (await _agentLogic.GetAllAccounts(0))?.ToList();
			
			if(customers == null || !customers.Any()) return;

			var opsTypes = Enum.GetValues(typeof(ExtractOperationType)).Cast<ExtractOperationType>();
			foreach (var opsType in opsTypes)
			{
				await CreateAndRunHeartbeatContainers(customers, opsType);
			}
        }
	}
}
