using System;

namespace NGraphics
{
	internal static class Log
	{
		public static void Error (Exception ex)
		{
			Console.WriteLine (ex);
		}
	}
}

