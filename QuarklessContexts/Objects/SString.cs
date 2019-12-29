using System.Linq;

namespace QuarklessContexts.Objects
{
	/// <summary>
	/// String object which are always lowered
	/// </summary>
	public class SString
	{
		private readonly string _string;
		public SString(string @string) => this._string = string.IsNullOrEmpty(@string) ? string.Empty : @string?.ToLower();

		public static implicit operator string(SString sString) => sString == null ? string.Empty : sString._string.ToLower();

		public static implicit operator SString(string @string) => new SString(string.IsNullOrEmpty(@string) ? string.Empty : @string?.ToLower());
		public int Len => _string.Length;
		public string String => _string;
		public bool IsEmptyOrBlank => string.IsNullOrEmpty(_string) && string.IsNullOrWhiteSpace(_string);
		public bool IsEmpty => string.IsNullOrEmpty(_string);
		public char Last => _string.LastOrDefault();
		public char First => _string.FirstOrDefault();
		public char this[int index] => _string[index];

	}
}
