using System;

namespace NGraphics
{
	public interface IImage //: IDrawable
	{
		void SaveAsPng (string path);
	}	

	public interface IImageCanvas : ICanvas
	{
		IImage GetImage ();
		Size ImageSize { get; }
		double ImageScale { get; }
	}
}
