using System;
using System.IO;
using System.Threading.Tasks;

namespace NGraphics
{
	public interface IImage //: IDrawable
	{
		Size Size { get; }
		double Scale { get; }
		void SaveAsPng (string path);
		void SaveAsPng (Stream stream);
	}	

	public interface IImageCanvas : ICanvas
	{
		IImage GetImage ();
		Size Size { get; }
		double Scale { get; }
	}
}
