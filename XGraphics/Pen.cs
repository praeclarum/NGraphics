using System;

namespace NGraphics
{
	public class Pen
	{
		public static Pen Black { get; private set; }

		static Pen ()
		{
			Black = new Pen ();
		}
	}
}

