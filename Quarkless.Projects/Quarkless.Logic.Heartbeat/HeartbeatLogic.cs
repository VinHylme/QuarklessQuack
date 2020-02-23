using Quarkless.Models.Heartbeat.Enums;
using Quarkless.Models.Heartbeat.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Heartbeat;
using Newtonsoft.Json;

namespace Quarkless.Logic.Heartbeat
{
	public class HeartbeatLogic : IHeartbeatLogic
	{
		private readonly IHeartbeatRepository _heartbeatRepository;
		public HeartbeatLogic(IHeartbeatRepository heartbeatRepository)
		{
			_heartbeatRepository = heartbeatRepository;
		}

		public async Task AddMetaData<TInput>(MetaDataCommitRequest<TInput> request)
		{
			var contains = await _heartbeatRepository.MetaDataContains(new MetaDataContainsRequest
			{
				InstagramId = request.InstagramId,
				ProfileCategoryTopicId = request.ProfileCategoryTopicId,
				MetaDataType = request.MetaDataType,
				JsonData = request.Data.ToJsonString()
			});

			if (!contains)
			{
				await _heartbeatRepository.AddMetaData(request);
			}
		}

		public async Task UpdateMetaData<TInput>(MetaDataCommitRequest<TInput> request)
		{
			var dataToRemove = (await _heartbeatRepository.GetMetaData<TInput>(new MetaDataFetchRequest
			{
				InstagramId = request.InstagramId,
				MetaDataType = request.MetaDataType,
				ProfileCategoryTopicId = request.ProfileCategoryTopicId
			}));
			
			var dto = dataToRemove.FirstOrDefault(_ => 
				JsonConvert.SerializeObject(_.ObjectItem) == JsonConvert.SerializeObject(request.Data.ObjectItem));

			if (dto!=null) {
				await _heartbeatRepository.DeleteMetaDataFromSet(request);
				await _heartbeatRepository.AddMetaData(request);
			}
		}

		public async Task<IEnumerable<Meta<TInput>>> GetMetaData<TInput>(MetaDataFetchRequest request)
		{
			return (await _heartbeatRepository.GetMetaData<TInput>(request))
				.DistinctBy(_ => _.ObjectItem);
		}

		public async Task RefreshMetaData(MetaDataFetchRequest request)
		{
			try {
				await _heartbeatRepository.RefreshMetaData(request);
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}

		public async Task<IEnumerable<TResults>> GetTempMetaData<TResults>(MetaDataTempType type)
			=> await _heartbeatRepository.GetTempMetaData<TResults>(type);
		public async Task DeleteMetaDataTemp(MetaDataTempType type)
			=> await _heartbeatRepository.DeleteMetaDataTemp(type);
	}
}
