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
//		readonly XNamespace ns;

		public SvgReader (System.IO.TextReader reader)
		{
			Read (XDocument.Load (reader));
		}

		void Read (XDocument doc)
		{
			var svg = doc.Root;
			var ns = svg.Name.Namespace;

			//
			// Find the defs (gradients)
			//
			foreach (var d in svg.Descendants ()) {
				var idA = d.Attribute ("id");
				if (idA != null) {
					defs [ReadString (idA).Trim ()] = d;
				}
			}

			//
			// Get the dimensions
			//
			var widthA = svg.Attribute ("width");
			var heightA = svg.Attribute ("height");
			var width = ReadNumber (widthA);
			var height = ReadNumber (heightA);
			var size = new Size (width, height);

			var viewBox = new Rect (size);
			var viewBoxA = svg.Attribute ("viewBox") ?? svg.Attribute ("viewPort");
			if (viewBoxA != null) {
				viewBox = ReadRectangle (viewBoxA.Value);
			}

			if (widthA != null && widthA.Value.Contains ("%")) {
				size.Width *= viewBox.Width;
			}
			if (heightA != null && heightA.Value.Contains ("%")) {
				size.Height *= viewBox.Height;
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
			case "path":
				{
					var dA = e.Attribute ("d");
					if (dA != null && !string.IsNullOrWhiteSpace (dA.Value)) {
						var p = new Path (pen, brush);
						ReadPath (p, dA.Value);
						r = p;
					}
				}
				break;
			case "g":
				{
					var g = new Group ();
					g.Transform = ReadTransform (ReadString (e.Attribute ("transform")));
					AddElements (g.Children, e.Elements ());
					r = g;
				}
				break;
			case "use":
				// Ignore multi layer for now
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

		Transform ReadTransform (string raw)
		{
			if (string.IsNullOrWhiteSpace (raw))
				return null;

			var s = raw.Trim ();

			var calls = s.Split (new[]{ ')' }, StringSplitOptions.RemoveEmptyEntries);

			Transform t = null;

			foreach (var c in calls) {
				var args = c.Split (new[]{ '(', ',', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				Transform nt = null;
				switch (args [0]) {
				case "translate":
					nt = new Translate (new Size (ReadNumber (args [1]), ReadNumber (args [2])), t);
					break;
				case "rotate":
					nt = new Rotate (ReadNumber (args [1]) * Math.PI / 180.0, t);
					break;
				default:
					throw new NotSupportedException ("Can't transform " + args[0]);
				}
				if (nt != null) {
					t = nt;
				}
			}

			return t;
		}

		static readonly char[] WSC = new char[] { ',', ' ', '\t', '\n', '\r' };

		void ReadPath (Path p, string pathDescriptor)
		{
			var args = pathDescriptor.Split (WSC, StringSplitOptions.RemoveEmptyEntries);

			var i = 0;
			var n = args.Length;

			while (i < n) {
				var a = args[i];
				//
				// Get the command
				//
				var cmd = "";
				if (a.Length == 1) {
					cmd = a;
					i++;
				} else {
					cmd = a.Substring (0, 1);
					args [i] = a.Substring (1);
				}

				//
				// Execute
				//
				if (cmd == "M" && i + 1 < n) {
					p.MoveTo (new Point (ReadNumber (args [i]), ReadNumber (args [i + 1])));
					i += 2;
				} else if (cmd == "L" && i + 1 < n) {
					p.LineTo (new Point (ReadNumber (args [i]), ReadNumber (args [i + 1])));
					i += 2;
				} else if (cmd == "C" && i + 5 < n) {
					var c1 = new Point (ReadNumber (args [i]), ReadNumber (args [i + 1]));
					var c2 = new Point (ReadNumber (args [i + 2]), ReadNumber (args [i + 3]));
					var pt = new Point (ReadNumber (args [i + 4]), ReadNumber (args [i + 5]));
					p.CurveTo (c1, c2, pt);
					i += 6;
				} else if (cmd == "z" || cmd == "Z") {
					p.Close ();
				} else {
					throw new NotSupportedException ("Path Command = " + cmd);
				}
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

			Color color;
			if (Colors.TryParse (stroke, out color)) {
				p.Color = color;
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

			Color color;
			if (Colors.TryParse (fill, out color)) {
				return new SolidBrush (color);
			}

			var urlM = fillUrlRe.Match (fill);
			if (urlM.Success) {
				var id = urlM.Groups [1].Value.Trim ();
				XElement defE;
				if (defs.TryGetValue (id, out defE)) {
					switch (defE.Name.LocalName) {
					case "linearGradient":
						return CreateLinearGradientBrush (defE);
					case "radialGradient":
						return CreateRadialGradientBrush (defE);
					default:
						throw new NotSupportedException ("Fill " + defE.Name);
					}
				} else {
					throw new Exception ("Invalid fill url reference: " + id);
				}
			}

			throw new NotSupportedException ("Fill " + fill);
		}

		RadialGradientBrush CreateRadialGradientBrush (XElement e)
		{
			var b = new RadialGradientBrush ();

			b.RelativeCenter.X = ReadNumber (e.Attribute ("cx"));
			b.RelativeCenter.Y = ReadNumber (e.Attribute ("cy"));
			b.RelativeFocus.X = ReadNumber (e.Attribute ("fx"));
			b.RelativeFocus.Y = ReadNumber (e.Attribute ("fy"));
			b.RelativeRadius = ReadNumber (e.Attribute ("r"));

			ReadStops (e, b.Stops);

			return b;
		}

		LinearGradientBrush CreateLinearGradientBrush (XElement e)
		{
			var b = new LinearGradientBrush ();

			b.RelativeStart.X = ReadNumber (e.Attribute ("x1"));
			b.RelativeStart.Y = ReadNumber (e.Attribute ("y1"));
			b.RelativeEnd.X = ReadNumber (e.Attribute ("x2"));
			b.RelativeEnd.Y = ReadNumber (e.Attribute ("y2"));

			ReadStops (e, b.Stops);

			return b;
		}

		void ReadStops (XElement e, List<GradientStop> stops)
		{
			var ns = e.Name.Namespace;
			foreach (var se in e.Elements (ns + "stop")) {
				var s = new GradientStop ();
				s.Offset = ReadNumber (se.Attribute ("offset"));
				s.Color = ReadColor (se, "stop-color");
				stops.Add (s);
			}
			stops.Sort ((x, y) => x.Offset.CompareTo (y.Offset));
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
				return Colors.Clear;

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
