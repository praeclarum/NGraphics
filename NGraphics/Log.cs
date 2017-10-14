using System;

namespace NGraphics
{
	internal static class Log
	{
		public static void Error (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine ("ERROR: " + ex);
		}
	}
}

