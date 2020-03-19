using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quarkless.Base.Proxy.Models.Enums;

namespace Quarkless.Base.Proxy.Models
{
	public class ProxyModel
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public string InstagramId { get; set; }
		public string AccountId { get; set; }

		/// <summary>
		/// This is specific to SOCKS5 proxies, as you will be connected to the local ip address
		/// (this object is here to say which proxy server it's connected to)
		/// </summary>
		public string IpAddressConnectedTo { get; set; }

		/// <summary>
		/// Normal IpAddress/url/host/etc to connect, local IP for SOCKS5
		/// </summary>
		public string HostAddress { get; set; }
		public int Port { get; set; }
		public bool NeedServerAuth { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		public Location Location { get; set; }
		public double Speed { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime? AssignedDate { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public ProxyType ProxyType { get; set; }
		public bool FromUser { get; set; }
	}
}