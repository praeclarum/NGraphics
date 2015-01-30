using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NGraphics
{
	public class SvgReader
	{
		public Graphic Graphic { get; private set; }
		public SvgReader (System.IO.TextReader reader)
		{
			var doc = XDocument.Load (reader);
			var size = Size.Zero;
			throw new NotImplementedException ();
			Graphic = new Graphic (size);
		}
	}

}
