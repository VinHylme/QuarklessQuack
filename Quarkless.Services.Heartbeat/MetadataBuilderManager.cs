using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QuarklessContexts.Models.Proxies;

namespace Quarkless.Services.Heartbeat
{
	public class MetadataBuilderManager
	{
		private readonly Assignment _assignment;
		private readonly IMetadataExtract _metadataExtract;
		public MetadataBuilderManager(IMetadataExtract metadataExtract, Assignment assignment)
		{
			_assignment = assignment;
			_metadataExtract = metadataExtract;
			_metadataExtract.Initialise(assignment);
		}

		/// <summary>
		/// Will begin work by fetching popular medias based on the users topic
		/// </summary>
		/// <returns></returns>
		public async Task BaseExtract()
		{
			Console.WriteLine("Began Base Extract for {0}", _assignment.Customer.InstagramAccount.Username);
			await _metadataExtract.BuildBase(3);
			Console.WriteLine("Ended Base Extract for {0}", _assignment.Customer.InstagramAccount.Username);
		}

		/// <summary>
		/// Will get images from outside based on the users profile
		/// </summary>
		/// <returns></returns>
		public async Task ExternalExtract()
		{
			Console.WriteLine("Began External Extract for {0}", _assignment.Customer.InstagramAccount.Username);
			await Task.Delay(3000);
			Console.WriteLine("Ended External Extract for {0}", _assignment.Customer.InstagramAccount.Username);
		}

		/// <summary>
		/// Will extract all users target listening based on profile (e.g. location and users)
		/// </summary>
		/// <returns></returns>
		public async Task TargetListingExtract()
		{
			Console.WriteLine("Began Target Extract for {0}", _assignment.Customer.InstagramAccount.Username);
			await Task.Delay(1000);
			Console.WriteLine("Ended Target Extract for {0}", _assignment.Customer.InstagramAccount.Username);
		}

		/// <summary>
		/// Will extract and update current users instagram profile
		/// </summary>
		/// <returns></returns>
		public async Task UserInformationExtract()
		{
			Console.WriteLine("Began User Info Extract for {0}", _assignment.Customer.InstagramAccount.Username);
			await Task.Delay(1000);
			Console.WriteLine("Ended User Info Extract for {0}", _assignment.Customer.InstagramAccount.Username);
		}
	}
}
