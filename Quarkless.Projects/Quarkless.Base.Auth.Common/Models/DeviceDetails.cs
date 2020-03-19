using Newtonsoft.Json;

namespace Quarkless.Base.Auth.Common.Models
{
	public class DeviceDetails
	{
		[JsonProperty("uniqueId")]
		public string UniqueId { get; set; }

		[JsonProperty("userAgent")]
		public string UserAgent { get; set; }

		[JsonProperty("webDriver")]
		public string WebDriver { get; set; }

		[JsonProperty("language")]
		public string Language { get; set; }

		[JsonProperty("colourDepth")]
		public int ColourDepth { get; set; }

		[JsonProperty("deviceMemory")]
		public int DeviceMemory { get; set; }

		[JsonProperty("hardwareConcurrency")]
		public int HardwareConcurrency { get; set; }

		[JsonProperty("screenResolution")]
		public ScreenWidth ScreenResolution { get; set; }

		[JsonProperty("availableScreenResolution")]
		public ScreenWidth AvailableScreenResolution { get; set; }

		[JsonProperty("timezoneOffset")]
		public int TimezoneOffset { get; set; }

		[JsonProperty("timezone")]
		public string Timezone { get; set; }

		[JsonProperty("sessionStorage")]
		public bool SessionStorage { get; set; }

		[JsonProperty("localStorage")]
		public bool LocalStorage { get; set; }

		[JsonProperty("indexedDatabase")]
		public bool IndexedDatabase { get; set; }

		[JsonProperty("addBehavior")]
		public bool AddBehavior { get; set; }

		[JsonProperty("openDatabase")]
		public bool OpenDatabase { get; set; }
		
		[JsonProperty("cpuClass")]
		public string CpuClass { get; set; }

		[JsonProperty("platform")]
		public string Platform { get; set; }

		[JsonProperty("plugins")]
		public string[] Plugins { get; set; }

		[JsonProperty("canvas")]
		public string[] Canvas { get; set; }

		[JsonProperty("webGl")]
		public string[] WebGl { get; set; }

		[JsonProperty("webGlVendor")]
		public string WebGlVendor { get; set; }

		[JsonProperty("adBlock")]
		public bool AdBlock { get; set; }
		
		[JsonProperty("hasLiedLanguages")]
		public bool HasLiedLanguages { get; set; }

		[JsonProperty("hasLiedResolution")]
		public bool HasLiedResolution { get; set; }

		[JsonProperty("hasLiedOs")]
		public bool HasLiedOs { get; set; }

		[JsonProperty("hasLiedBrowser")]
		public bool HasLiedBrowser { get; set; }

		[JsonProperty("touchSupport")]
		public bool TouchSupport { get; set; }

		[JsonProperty("fonts")]
		public string[] Fonts { get; set; }

		[JsonProperty("audio")]
		public double Audio { get; set; }
	}
}