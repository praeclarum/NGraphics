using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

namespace NGraphics
{
	public class SvgWriter : IElementVisitor
	{
		public Graphic Graphic { get; private set; }

		readonly CodeWriter w;

		readonly Dictionary<object, string> defs = new Dictionary<object, string> ();

		bool wrote = false;

		public SvgWriter (Graphic graphic, System.IO.TextWriter writer)
		{
			Graphic = graphic;
			w = new CodeWriter (writer, "    ");
		}

		public void Write ()
		{
			if (wrote)
				return;
			wrote = true;

			w.WriteLine ("<?xml version=\"1.0\" encoding=\"{0}\" standalone=\"no\"?>", w.Encoding.WebName.ToUpperInvariant ());
			w.WriteLine ("<svg width=\"{0}px\" height=\"{1}px\" viewBox=\"{2} {3} {4} {5}\" version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\">",
				Graphic.Size.Width, Graphic.Size.Height,
				Graphic.ViewBox.X, Graphic.ViewBox.Y,
				Graphic.ViewBox.Width, Graphic.ViewBox.Height);
			w.Indent ();
			w.WriteLine ("<title>{0}</title>", Escape (Graphic.Title));
			w.WriteLine ("<description>{0}</description>", Escape (Graphic.Description));
			w.WriteLine ("<defs>");
			w.Indent ();
			var dw = new DefsWriter { w = w, defs = defs };
			foreach (var c in Graphic.Children) {
				c.Accept (dw);
			}
			w.Outdent ();
			w.WriteLine ("</defs>");
			foreach (var c in Graphic.Children) {
				c.Accept (this);
			}
			w.Outdent ();
			w.WriteLine ("</svg>");
		}

		class DefsWriter : BaseElementVisitor
		{
			int nextId = 1;
			public CodeWriter w;
			public Dictionary<object, string> defs;
			public override void VisitElement (Element element)
			{
				var lgb = element.Brush as LinearGradientBrush;
				if (lgb != null && !defs.ContainsKey (lgb)) {
					var id = "linearGradient-" + (nextId++);
					defs [lgb] = id;
					w.WriteLine ("<linearGradient x1=\"{1}{5}\" y1=\"{2}{5}\" x2=\"{3}{5}\" y2=\"{4}{5}\" id=\"{0}\">",
						id,
						lgb.Start.X*100, lgb.Start.Y*100,
						lgb.End.X*100, lgb.End.Y*100,
						lgb.Absolute ? "" : "%");
					WriteStops (lgb);
					w.WriteLine ("</linearGradient>");
				}
				var rgb = element.Brush as RadialGradientBrush;
				if (rgb != null && !defs.ContainsKey (rgb)) {
					var id = "radialGradient-" + (nextId++);
					defs [rgb] = id;
					w.WriteLine ("<radialGradient cx=\"{1}{6}\" cy=\"{2}{6}\" fx=\"{3}{6}\" fy=\"{4}{6}\" r=\"{5}{6}\" id=\"{0}\">",
						id,
						rgb.Center.X*100, rgb.Center.Y*100,
						rgb.Focus.X*100, rgb.Focus.Y*100,
						rgb.Radius.Width*100,
						rgb.Absolute ? "" : "%");
					WriteStops (rgb);
					w.WriteLine ("</radialGradient>");
				}
			}

			void WriteStops (GradientBrush gb)
			{
				w.Indent ();
				foreach (var s in gb.Stops) {
					w.Write ("<stop stop-color=\"{0}\" offset=\"{1}%\"", s.Color.HtmlString, s.Offset*100);
					if (s.Color.A != 255) {
						w.Write (" stop-opacity=\"{0}\"", s.Color.Alpha);
					}
					w.WriteLine (" />", s.Color.HtmlString, s.Offset*100);
				}
				w.Outdent ();
			}
		}

		void WriteElement (Element c)
		{
			w.WriteLine (c.ToString ());
		}

		string Escape (string text)
		{
			return (text??"").Replace ("&", "&amp;").Replace ("<", "&lt;").Replace (">", "&gt;");
		}

		void WriteStartElement (string name, Element element)
		{
			w.Write ("<{0}", name);
			if (!string.IsNullOrWhiteSpace (element.Id)) {
				w.Write (" id=\"{0}\"", element.Id);
			}
			if (element.Pen != null) {
				w.Write (" stroke-width=\"{0}\"", element.Pen.Width);
				w.Write (" stroke=\"{0}\"", element.Pen.Color.HtmlString);
				if (element.Pen.Color.A != 255) {
					w.Write (" stroke-opacity=\"{0}\"", element.Pen.Color.Alpha);
				}
			} else {
				w.Write (" stroke=\"none\"");
			}
			if (element.Brush != null) {
				//fill-rule="evenodd"
				var sb = element.Brush as SolidBrush;
				if (sb != null) {
					w.Write (" fill=\"{0}\"", sb.Color.HtmlString);
					if (sb.Color.A != 255) {
						w.Write (" fill-opacity=\"{0}\"", sb.Color.Alpha);
					}
				} else {
					w.Write (" fill=\"url(#{0})\"", defs[element.Brush]);
				}
			} else {
				w.Write (" fill=\"none\"");
			}
			var t = element.Transform;
			if (t != Transform.Identity) {
				w.Write (" transform=\"matrix({0}, {1}, {2}, {3}, {4}, {5})\"",
					t.A, t.B,
					t.C, t.D,
					t.E, t.F);
			}
		}

		#region IElementVisitor implementation

		public void Visit (Group group)
		{
			WriteStartElement ("g", group);
			w.WriteLine (">");
			w.Indent ();
		}

		public void EndVisit (Group group)
		{
			w.Outdent ();
			w.WriteLine ("</g>");
		}

		public void Visit (Rectangle rectangle)
		{
			WriteStartElement ("rect", rectangle);
			w.Write (" x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\"",
				rectangle.Frame.X, rectangle.Frame.Y,
				rectangle.Frame.Width, rectangle.Frame.Height);
		}

		public void EndVisit (Rectangle rectangle)
		{
			w.WriteLine (" />");
		}

		public void Visit (Ellipse ellipse)
		{
			WriteStartElement ("ellipse", ellipse);
			w.Write (" cx=\"{0}\" cy=\"{1}\" rx=\"{2}\" ry=\"{3}\"",
				ellipse.Frame.Center.X, ellipse.Frame.Center.Y,
				ellipse.Frame.Width/2, ellipse.Frame.Height/2);
		}

		public void EndVisit (Ellipse e)
		{
			w.WriteLine (" />");
		}

		public void Visit (Path path)
		{
			WriteStartElement ("path", path);
			w.Write (" d=\"");
			var opw = new PathOpWriter { w = w };
			path.AcceptPathOpVisitor (opw);
			w.Write ("\"");
		}

		class PathOpWriter : IPathOpVisitor
		{
			public CodeWriter w;

			#region IPathOpVisitor implementation

			public void Visit (MoveTo moveTo)
			{
				w.Write ("M{0},{1} ", moveTo.Point.X, moveTo.Point.Y);
			}

			public void Visit (LineTo lineTo)
			{
				w.Write ("L{0},{1} ", lineTo.Point.X, lineTo.Point.Y);
			}

			public void Visit (CurveTo curveTo)
			{
				w.Write ("C{0},{1} {2},{3} {4},{5} ",
					curveTo.Control1.X, curveTo.Control1.Y,
					curveTo.Control2.X, curveTo.Control2.Y,
					curveTo.Point.X, curveTo.Point.Y);
			}

			public void Visit (ArcTo arcTo)
			{
				w.Write ("A{0} {1} {2} {3} {4} {5} {6} ",
					arcTo.Radius.Width,
					arcTo.Radius.Height,
					0,
					arcTo.LargeArc ? 1 : 0,
					arcTo.SweepClockwise ? 1 : 0,
					arcTo.Point.X, arcTo.Point.Y);
			}

			public void Visit (ClosePath closePath)
			{
				w.Write ("Z ");
			}

			#endregion
		}

		public void EndVisit (Path e)
		{
			w.WriteLine (" />");
		}

		public void Visit (Text text)
		{
			WriteStartElement ("text", text);
			w.Write (" x=\"{0}\" y=\"{1}\"",
				text.Frame.X, text.Frame.Y);
			if (text.Frame.Width != double.MaxValue || text.Frame.Height != double.MaxValue) {
				w.Write (" width=\"{0}\" height=\"{1}\"",
					text.Frame.Width, text.Frame.Height);
			}
			if (text.Font != null) {
				w.Write (" font-family=\"{0}\" font-size=\"{1}\"",
					text.Font.Name, text.Font.Size);
			}
			w.Write (">");
			w.Write (Escape (text.String));
		}

		public void EndVisit (Text e)
		{
			w.WriteLine ("</text>");
		}

		public void Visit (ForeignObject foreignObject)
		{
			WriteStartElement ("foreignObject", foreignObject);
		}

		public void EndVisit (ForeignObject e)
		{
			w.WriteLine (" />");
		}

		#endregion
	}


//	public class TestSvgWriter
//	{
//		public string Result;
//		public TestSvgWriter ()
//		{
//			var g = Graphic.LoadSvg (new StreamReader ("/Users/fak/Dropbox/Projects/NGraphics/NGraphics.Test/Inputs/MocastIcon.svg"));
//			var w = new StringWriter ();
//			var sw = new SvgWriter (g, w);
//			sw.Write ();
//			System.Console.WriteLine (w);
//			Result = w.ToString ();
//		}
//	}
}