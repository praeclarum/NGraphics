using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Shapes = Windows.UI.Xaml.Shapes;

namespace NGraphics
{
	/// <summary>
	/// Wraps a XAML Image Source.
	/// </summary>
	public class ImageSourceImage : IImage
	{
		ImageSource source;

		public ImageSourceImage (ImageSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			this.source = source;
		}
		public void SaveAsPng (string path)
		{
			throw new NotSupportedException ("WinRT does not support saving to files. Please use the Stream override instead.");
		}
		public async Task SaveAsPngAsync (Stream stream)
		{
			var enc = await BitmapEncoder.CreateAsync (BitmapEncoder.PngEncoderId, stream.AsRandomAccessStream (), null);
			await enc.FlushAsync ();
		}
	}
}
