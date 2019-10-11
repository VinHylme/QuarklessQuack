namespace QuarklessContexts.Models
{
	public struct Range
	{
		public int Min;
		public int Max;
		public Range(int min, int max)
		{
			this.Min = min;
			this.Max = max;
		}
	}
}
