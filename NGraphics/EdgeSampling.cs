using System;

namespace NGraphics
{
	public struct EdgeSamples
	{
		public Point[] Points;

		public double DistanceTo (Point p)
		{
			var minD = double.MaxValue;
			var n = Points.Length;
			for (var i = 0; i < (n - 1); i++) {
				var d = p.DistanceToLineSegment (Points [i], Points [i + 1]);
				if (d < minD)
					minD = d;
			}
			return minD;
		}
	}

	public interface IEdgeSampleable
	{		
		Rect SampleableBox { get; }
		EdgeSamples[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples);
	}

	public static class Sampling
	{
		public static EdgeSamples[] SampleEdges (this IEdgeSampleable s)
		{
			var tolerance = s.SampleableBox.Size.Diagonal / 10;
			return s.GetEdgeSamples (tolerance, 2, 10);
		}
	}
}

