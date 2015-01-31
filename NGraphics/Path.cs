using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public abstract class PathCommand
	{
	}
	public class MoveTo : PathCommand
	{
		public Point Point;
		public MoveTo (Point point)
		{
			Point = point;
		}
	}
	public class LineTo : PathCommand
	{
		public Point Point;
		public LineTo (Point point)
		{
			Point = point;
		}
	}
	public class CurveTo : PathCommand
	{
		public Point Control1;
		public Point Control2;
		public Point Point;
		public CurveTo (Point control1, Point control2, Point point)
		{
			Control1 = control1;
			Control2 = control2;
			Point = point;
		}
	}
	public class ClosePath : PathCommand
	{
	}

	public class Path : Element
	{
		public readonly List<PathCommand> Commands = new List<PathCommand> ();

		public bool IsClosed {
			get {
				return Commands.Count > 0 && Commands [Commands.Count - 1] is ClosePath;
			}
		}

		public Path (IEnumerable<PathCommand> commands, Pen pen = null, Brush brush = null)
			: base (pen, brush)
		{
			Commands.AddRange (commands);
		}
		public Path (Pen pen = null, Brush brush = null)
			: base (pen, brush)
		{
		}

		protected override void DrawElement (ICanvas canvas)
		{
			canvas.DrawPath (Commands, Pen, Brush);
		}

		void Add (PathCommand cmd)
		{
			if (IsClosed)
				return;
			Commands.Add (cmd);
		}

		public void MoveTo (Point point)
		{
			Add (new MoveTo (point));
		}

		public void LineTo (Point point)
		{
			Add (new LineTo (point));
		}

		public void CurveTo (Point control1, Point control2, Point point)
		{
			Add (new CurveTo (control1, control2, point));
		}

		public void Close ()
		{
			Add (new ClosePath ());
		}

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "Path ([...])");
		}
	}
}

