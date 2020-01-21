namespace Quarkless.Models.Common.Models
{
	public class Location
	{
		public string City { get; set; }
		public string Address { get; set; }
		public string PostCode { get; set; }
		public Coordinates Coordinates { get; set; }
	}
}