using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NGraphics
{
	public class SvgReader
	{
		readonly IFormatProvider icult = System.Globalization.CultureInfo.InvariantCulture;

		public Graphic Graphic { get; private set; }

		readonly Dictionary<string, XElement> defs = new Dictionary<string, XElement> ();
		readonly XNamespace ns;

		public SvgReader (System.IO.TextReader reader)
		{
			var doc = XDocument.Load (reader);
			var svg = doc.Root;
			ns = svg.Name.Namespace;

			//
			// Find the defs (gradients)
			//
			foreach (var d in svg.Descendants (ns + "defs").SelectMany (x => x.Elements ())) {
				defs [ReadString (d.Attribute ("id")).Trim ()] = d;
			}

			//
			// Get the dimensions
			//
			var width = ReadNumber (svg.Attribute ("width"));
			var height = ReadNumber (svg.Attribute ("height"));
			var size = new Size (width, height);

			var viewBox = new Rect (size);
			var viewBoxA = svg.Attribute ("viewBox") ?? svg.Attribute ("viewPort");
			if (viewBoxA != null) {
				viewBox = ReadRectangle (viewBoxA.Value);
			}

			//
			// Add the elements
			//
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
			if (pen == null && brush == null) {
				brush = Brushes.Black;
			}
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
			case "circle":
				{
					var cx = ReadNumber (e.Attribute ("cx"));
					var cy = ReadNumber (e.Attribute ("cy"));
					var rr = ReadNumber (e.Attribute ("r"));
					r = new Ellipse (new Point (cx - rr, cy - rr), new Size (2 * rr, 2 * rr), pen, brush);
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
			var stroke = ReadString (e.Attribute ("stroke"), "none").Trim ();
			if (stroke == "none" || string.IsNullOrEmpty (stroke))
				return null;

			var p = new Pen ();

			if (stroke [0] == '#' && stroke.Length == 7) {
				p.Color = ReadColor (stroke);
			} else {
				throw new NotSupportedException ("Stroke " + stroke);
			}

			var strokeWidthA = e.Attribute ("stroke-width");
			if (strokeWidthA != null) {
				p.Width = ReadNumber (strokeWidthA);
			}

			return p;
		}

		Regex fillUrlRe = new Regex (@"url\s*\(\s*#([^\)]+)\)");

		Brush ReadBrush (XElement e)
		{
			var fill = ReadString (e.Attribute ("fill"), "none").Trim ();
			if (fill == "none" || string.IsNullOrEmpty (fill))
				return null;

			var urlM = fillUrlRe.Match (fill);
			if (urlM.Success) {
				var id = urlM.Groups [1].Value.Trim ();
				XElement defE;
				if (defs.TryGetValue (id, out defE)) {
					switch (defE.Name.LocalName) {
					case "linearGradient":
						return CreateLinearGradientBrush (defE);
					default:
						throw new NotSupportedException ("Fill " + defE.Name);
					}
				} else {
					throw new Exception ("Invalid fill url reference: " + id);
				}
			}

			throw new NotSupportedException ("Fill " + fill);
		}

		LinearGradientBrush CreateLinearGradientBrush (XElement e)
		{
			var b = new LinearGradientBrush ();

			b.RelativeStart.X = ReadNumber (e.Attribute ("x1"));
			b.RelativeStart.Y = ReadNumber (e.Attribute ("y1"));
			b.RelativeEnd.X = ReadNumber (e.Attribute ("x2"));
			b.RelativeEnd.Y = ReadNumber (e.Attribute ("y2"));

			foreach (var se in e.Elements (ns + "stop")) {
				var s = new GradientStop ();
				s.Offset = ReadNumber (se.Attribute ("offset"));
				s.Color = ReadColor (se, "stop-color");
				b.Stops.Add (s);
			}

			b.Stops.Sort ((x, y) => x.Offset.CompareTo (y.Offset));

			return b;
		}

		Color ReadColor (XElement e, string attrib)
		{
			var a = e.Attribute (attrib);
			if (a == null)
				return Colors.Black;
			return ReadColor (a.Value);
		}

		Color ReadColor (string raw)
		{
			if (string.IsNullOrWhiteSpace (raw))
				return Colors.Black;

			var s = raw.Trim ();

			if (s.Length == 7 && s [0] == '#') {

				var r = int.Parse (s.Substring (1, 2), NumberStyles.HexNumber, icult);
				var g = int.Parse (s.Substring (3, 2), NumberStyles.HexNumber, icult);
				var b = int.Parse (s.Substring (5, 2), NumberStyles.HexNumber, icult);

				return new Color (r / 255.0, g / 255.0, b / 255.0, 1);

			}

			throw new NotSupportedException ("Color " + s);
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
			else if (s.EndsWith ("%")) {
				s = s.Substring (0, s.Length - 1);
				m = 0.01;
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
