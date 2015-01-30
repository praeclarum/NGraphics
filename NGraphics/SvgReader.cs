using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using System.Diagnostics;

namespace NGraphics
{
	public class SvgReader
	{
		IFormatProvider icult = System.Globalization.CultureInfo.InvariantCulture;

		public Graphic Graphic { get; private set; }

		public SvgReader (System.IO.TextReader reader)
		{
			var doc = XDocument.Load (reader);
			var svg = doc.Root;
			var ns = svg.Name.Namespace;

			var width = ReadNumber (svg.Attribute ("width"));
			var height = ReadNumber (svg.Attribute ("height"));
			var size = new Size (width, height);

			var viewBox = new Rect (size);
			var viewBoxA = svg.Attribute ("viewBox") ?? svg.Attribute ("viewPort");
			if (viewBoxA != null) {
				viewBox = ReadRectangle (viewBoxA.Value);
			}

			Graphic = new Graphic (size, viewBox);

			AddElements (Graphic.Children, svg.Elements ());
		}

		void AddElements (IList<IDrawable> list, IEnumerable<XElement> es)
		{
			foreach (var e in es)
				AddElement (list, e);
		}

		void AddElement (IList<IDrawable> list, XElement e)
		{
			IDrawable r = null;
			var pen = ReadPen (e);
			var brush = ReadBrush (e);
			//var id = ReadString (e.Attribute ("id"));
			switch (e.Name.LocalName) {
			case "rect":
				{
					var x = ReadNumber (e.Attribute ("x"));
					var y = ReadNumber (e.Attribute ("y"));
					var width = ReadNumber (e.Attribute ("width"));
					var height = ReadNumber (e.Attribute ("height"));
					r = new Rectangle (new Point (x, y), new Size (width, height), pen, brush);
				}
				break;
			case "ellipse":
				{
					var cx = ReadNumber (e.Attribute ("cx"));
					var cy = ReadNumber (e.Attribute ("cy"));
					var rx = ReadNumber (e.Attribute ("rx"));
					var ry = ReadNumber (e.Attribute ("ry"));
					r = new Ellipse (new Point (cx - rx, cy - ry), new Size (2 * rx, 2 * ry), pen, brush);
				}
				break;
			case "g":
				{
					var g = new Group ();
					AddElements (g.Children, e.Elements ());
					r = g;
				}
				break;
			case "title":
				Graphic.Title = ReadString (e);
				break;
			case "description":
				Graphic.Description = ReadString (e);
				break;
			case "defs":
				// Already read in earlier pass
				break;
			default:
				throw new NotSupportedException ("SVG element \"" + e.Name.LocalName + "\" is not supported");
			}

			if (r != null) {
				list.Add (r);
			}
		}

		string ReadString (XElement e, string defaultValue = "")
		{
			if (e == null)
				return defaultValue;
			return e.Value ?? defaultValue;
		}

		string ReadString (XAttribute a, string defaultValue = "")
		{
			if (a == null)
				return defaultValue;
			return a.Value ?? defaultValue;
		}

		Pen ReadPen (XElement e)
		{
			return null;
		}

		Brush ReadBrush (XElement e)
		{
			return Brushes.Black;
		}

		double ReadNumber (XAttribute a)
		{
			if (a == null)
				return 0;
			return ReadNumber (a.Value);
		}

		double ReadNumber (string raw)
		{
			if (string.IsNullOrWhiteSpace (raw))
				return 0;

			var s = raw.Trim ();
			var m = 1.0;

			if (s.EndsWith ("px")) {
				s = s.Substring (0, s.Length - 2);
			}

			double v;
			if (!double.TryParse (s, NumberStyles.Float, icult, out v)) {
				v = 0;
			}
			return m * v;
		}

		static readonly char[] WS = new char[] { ' ', '\t', '\n', '\r' };

		Rect ReadRectangle (string s)
		{
			var r = new Rect ();
			var p = s.Split (WS, StringSplitOptions.RemoveEmptyEntries);
			if (p.Length > 0)
				r.X = ReadNumber (p [0]);
			if (p.Length > 1)
				r.Y = ReadNumber (p [1]);
			if (p.Length > 2)
				r.Width = ReadNumber (p [2]);
			if (p.Length > 3)
				r.Height = ReadNumber (p [3]);
			return r;
		}
	}
}
