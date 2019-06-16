using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.DatabaseModels
{
	public class PhoneContact
	{
		public string ContactPhone { get; set; }
		public string CountryCode { get; set; }
		public string PublicPhone { get; set; }
	}
	public class Contact
	{
		public string CityName { get; set; }
		public string Address { get; set; }
		public long CityId { get; set; }
		public PhoneContact ContactPhone { get; set; }
		public string ContactEmail { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public string ZipCode { get; set; }
	}
	public class User
	{
		public long UserId { get; set; }
		public string Username { get; set; }
		public long FollowerCount { get; set; }
		public long FollowingCount { get; set; }
		public string FullName { get; set; }
	}
}
