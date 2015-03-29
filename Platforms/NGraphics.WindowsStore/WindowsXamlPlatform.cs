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
	public class WindowsXamlPlatform : IPlatform
	{
		public string Name { get { return "WindowsXaml"; } }

		public IImageCanvas CreateImageCanvas (Size size, double scale = 1.0, bool transparency = true)
		{
			throw new NotImplementedException ();
		}

		public IImage CreateImage (Color[] colors, int width, double scale = 1.0)
		{
			throw new NotImplementedException ();
		}

		public IImage LoadImage (Stream stream)
		{
			throw new NotImplementedException ();
		}
		public IImage LoadImage (string path)
		{
			throw new NotImplementedException ();
		}
	}
}
