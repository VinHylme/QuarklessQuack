﻿using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Heartbeat.Models.Interfaces
{
	public interface IMetaDataRequest
	{
		MetaDataType MetaDataType { get; set; }
		string ProfileCategoryTopicId { get; set; }
		string InstagramId { get; set; }
		string AccountId { get; set; }
	}
}