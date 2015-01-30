using NUnit.Framework;
using System.IO;
using System;
using System.Reflection;

namespace NGraphics.Test
{
	[TestFixture]
	public class SvgReaderTests
	{
		Stream OpenResource (string path)
		{
			var assembly = typeof (SvgReaderTests).GetTypeInfo ().Assembly;
			var resources = assembly.GetManifestResourceNames ();
			return assembly.GetManifestResourceStream ("NGraphics.Test.Inputs." + path);
		}

		void Read (string path)
		{
			using (var s = OpenResource ("mozilla.ellipse.svg")) {
				var r = new SvgReader (new StreamReader (s));
				Assert.IsNotNull (r);
			}

		}

		[Test]
		public void ReadMozillaEllipse ()
		{
			Read ("mozilla/ellipse.svg");
		}
	}
}

