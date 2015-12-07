using System;

namespace NGraphics
{
	public interface IEdgeSampleable
	{		
		Rect SampleableBox { get; }
		Point[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples);
	}

	public static class Sampling
	{
		public static Point[] SampleEdges (this IEdgeSampleable s)
		{
			var tolerance = s.SampleableBox.Size.Diagonal / 10;
			return s.GetEdgeSamples (tolerance, 2, 10);
		}
	}
}

