using System;
using System.IO;
using System.Threading.Tasks;

namespace NGraphics
{
	public interface IImage //: IDrawable
	{
		void SaveAsPng (string path);
		Task SaveAsPngAsync (Stream stream);
	}	

	public interface IImageCanvas : ICanvas
	{
		Task<IImage> GetImageAsync ();
		Size Size { get; }
		double Scale { get; }
	}
}
