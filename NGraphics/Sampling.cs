using System;

namespace NGraphics
{
	public interface ISampleable
	{		
		Rect SampleableBox { get; }
		Point[] GetSamples (double tolerance, int minSamples, int maxSamples);
	}

	public static class Sampling
	{
		public static Point[] Sample (this ISampleable s)
		{
			var tolerance = s.SampleableBox.Size.Diagonal / 10;
			return s.GetSamples (tolerance, 2, 10);
		}
	}
}

