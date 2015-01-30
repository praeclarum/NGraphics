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

			var width = ReadNumber (svg.Attribute ("width"));
			var height = ReadNumber (svg.Attribute ("height"));
			var size = new Size (width, height);

			var viewPort = new Rectangle (size);
			var viewPortA = svg.Attribute ("viewPort");
			if (viewPortA != null) {
				viewPort = ReadRectangle (viewPortA.Value);
			}

			Graphic = new Graphic (size, viewPort);

			foreach (var e in svg.Elements ())
				AddElement (e);
		}

		void AddElement (XElement e)
		{
			IDrawable r = null;
			var pen = ReadPen (e);
			var brush = ReadBrush (e);
			//var id = ReadString (e.Attribute ("id"));
			switch (e.Name.LocalName) {
			case "ellipse":
				{
					var cx = ReadNumber (e.Attribute ("cx"));
					var cy = ReadNumber (e.Attribute ("cy"));
					var rx = ReadNumber (e.Attribute ("rx"));
					var ry = ReadNumber (e.Attribute ("ry"));
					r = new Ellipse (new Point (cx - rx, cy - ry), new Size (2 * rx, 2 * ry), pen, brush);
				}
				break;
			default:
				throw new NotImplementedException ();
			}

			if (r != null) {
				Graphic.Children.Add (r);
			}
		}

		string ReadString (XAttribute a, string defaultValue = "")
		{
			if (a == null)
				return defaultValue;
			return a.Value;
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

		double ReadNumber (string s)
		{
			if (s == null)
				return 0;
			double v;
			if (!double.TryParse (s, NumberStyles.Float, icult, out v)) {
				v = 0;
			}
			return v;
		}

		static readonly char[] WS = new char[] { ' ', '\t', '\n', '\r' };

		Rectangle ReadRectangle (string s)
		{
			var r = new Rectangle ();
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
