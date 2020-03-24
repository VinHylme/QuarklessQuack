namespace Quarkless.Base.Analytics.Models
{
	public class RelatedHashtag
	{
		public long Id { get; set; }
		public string TagName { get; set; }
		public string Type { get; set; }
		public double SimilarityDistanceToParent { get; set; }
	}
}