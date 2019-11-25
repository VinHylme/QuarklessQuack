namespace Quarkless.Analyser
{
	public interface IMediaManipulation
	{
		IVideoEditor VideoEditor { get; }
		IImageEditor ImageEditor { get; }
	}
}