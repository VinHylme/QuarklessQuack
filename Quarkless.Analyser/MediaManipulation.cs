namespace Quarkless.Analyser
{
	public class MediaManipulation : IMediaManipulation
	{
		public IVideoEditor VideoEditor { get; }
		public IImageEditor ImageEditor => new ImageEditor();
		public MediaManipulation(IVideoEditor videoEditor)
		{
			VideoEditor = videoEditor;
		}

	}
}
