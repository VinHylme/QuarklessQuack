﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quarkless.Base.Proxy.Models.Enums;

namespace Quarkless.Base.Proxy.Models
{
	public class Location
	{
		public string LocationQuery { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		[BsonRepresentation(BsonType.Int32)]
		public CountryCode CountryCode { get; set; }
		public string PostalCode { get; set; }
		public string FullAddress { get; set; }
		public string City { get; set; }
		public string State { get; set; }
	}
}