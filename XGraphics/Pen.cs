using System;

namespace XGraphics
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

