namespace QuarklessLogic.Handlers.TextGeneration
{
	public interface ITextGeneration
	{
		string MarkovTextGenerator(string filePath, int limit, int size, bool exact = false);
		string MarkovTextGenerator(string filePath, int type, string topic, string language, int size, int limit);
	}
}