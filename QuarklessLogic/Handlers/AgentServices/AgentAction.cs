using InstagramApiSharp.Classes.Models;
using QuarklessLogic.Handlers.ClientProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessLogic.Handlers.AgentServices
{
	public class AgentAction
	{
		private readonly IAPIClientContainer _client;

		public AgentAction(IAPIClientContainer client)
		{
			_client = client;
		}

		public async void Post()
		{
			InstaImageUpload instaImageUpload = new InstaImageUpload
			{
				
			};
			string caption = string.Empty;
			InstaLocationShort instaLocation = new InstaLocationShort
			{
				
			};

			await _client.Media.UploadPhotoAsync(instaImageUpload,caption,instaLocation);
		}

		public void Comment()
		{

		}

		public void Follow()
		{

		}

	}
}
