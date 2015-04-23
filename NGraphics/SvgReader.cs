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

			AddElements (Graphic.Children, svg.Elements (), null, Brushes.Black);
		}

		void AddElements (IList<IDrawable> list, IEnumerable<XElement> es, Pen inheritPen, Brush inheritBrush)
		{
			foreach (var e in es)
				AddElement (list, e, inheritPen, inheritBrush);
		}

		void AddElement (IList<IDrawable> list, XElement e, Pen inheritPen, Brush inheritBrush)
		{
			//
			// Style
			//
			Element r = null;
			Pen pen = null;
			Brush brush = null;
			bool hasPen, hasBrush;
			ApplyStyle (e.Attributes ().ToDictionary (k => k.Name.LocalName, v => v.Value), ref pen, out hasPen, ref brush, out hasBrush);
			var style = ReadString (e.Attribute ("style"));
			if (!string.IsNullOrWhiteSpace (style)) {
				ApplyStyle (style, ref pen, out hasPen, ref brush, out hasBrush);
			}
			pen = hasPen ? pen : inheritPen;
			brush = hasBrush ? brush : inheritBrush;
			//var id = ReadString (e.Attribute ("id"));

			//
			// Elements
			//
			switch (e.Name.LocalName) {
			case "text":
				{
					var x = ReadNumber (e.Attribute ("x"));
					var y = ReadNumber (e.Attribute ("y"));
					var text = e.Value.Trim ();
					var fontFamilyAttribute = e.Attribute("font-family");
					var font = new Font ();
					if (fontFamilyAttribute != null)
						font.Family = fontFamilyAttribute.Value.Trim('\'');
					var fontSizeAttribute = e.Attribute("font-size");
					if (fontSizeAttribute != null)
						font.Size = ReadNumber(fontSizeAttribute.Value);
					r = new Text (text, new Rect (new Point (x, y), new Size (double.MaxValue, double.MaxValue)), font, TextAlignment.Left, pen, brush);
				}
				break;
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
			case "polygon":
				{
					var pA = e.Attribute ("points");
					if (pA != null && !string.IsNullOrWhiteSpace (pA.Value)) {
						var path = new Path (pen, brush);
						ReadPolygon (path, pA.Value);
						r = path;
					}
				}
				break;
			case "g":
				{
					var g = new Group ();
					AddElements (g.Children, e.Elements (), pen, brush);
					r = g;
				}
				break;
			case "use":
				{
					var href = ReadString (e.Attributes ().FirstOrDefault (x => x.Name.LocalName == "href"));
					if (!string.IsNullOrWhiteSpace (href)) {
						XElement useE;
						if (defs.TryGetValue (href.Trim ().Replace ("#", ""), out useE)) {
							var useList = new List<IDrawable> ();
							AddElement (useList, useE, pen, brush);
							r = useList.OfType<Element> ().FirstOrDefault ();
						}
					}
				}
				break;
			case "title":
				Graphic.Title = ReadString (e);
				break;
			case "desc":
			case "description":
				Graphic.Description = ReadString (e);
				break;
			case "defs":
				// Already read in earlier pass
				break;
			case "namedview":
			case "metadata":
			case "image":
				// Ignore
				break;

				case "line":
				{
					var x1 = ReadNumber ( e.Attribute("x1") );
					var x2 = ReadNumber ( e.Attribute("x2") );
					var y1 = ReadNumber ( e.Attribute("y1") );
					var y2 = ReadNumber ( e.Attribute("y2") );
					r = new Line(new Point(x1, y1), new Point(x2, y2), pen);
				}
				break;


				// color definition that can be referred to by other elements
				case "linearGradient":
				break;


			default:
				throw new NotSupportedException ("SVG element \"" + e.Name.LocalName + "\" is not supported");
			}

			if (r != null) {
				r.Transform = ReadTransform (ReadString (e.Attribute ("transform")));
				list.Add (r);
			}
		}

		Regex keyValueRe = new Regex (@"\s*(\w+)\s*:\s*(.*)");

		void ApplyStyle (string style, ref Pen pen, out bool hasPen, ref Brush brush, out bool hasBrush)
		{
			var d = new Dictionary<string, string> ();
			var kvs = style.Split (new[]{ ';' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var kv in kvs) {
				var m = keyValueRe.Match (kv);
				if (m.Success) {
					var k = m.Groups [1].Value;
					var v = m.Groups [2].Value;
					d [k] = v;
				}
			}
			ApplyStyle (d, ref pen, out hasPen, ref brush, out hasBrush);
		}

		string GetString (Dictionary<string, string> style, string name, string defaultValue = "")
		{
			string v;
			if (style.TryGetValue (name, out v))
				return v;
			return defaultValue;
		}

		Regex fillUrlRe = new Regex (@"url\s*\(\s*#([^\)]+)\)");

		void ApplyStyle (Dictionary<string, string> style, ref Pen pen, out bool hasPen, ref Brush brush, out bool hasBrush)
		{
			//
			// Pen attributes
			//
			var strokeWidth = GetString (style, "stroke-width");
			if (!string.IsNullOrWhiteSpace (strokeWidth)) {
				if (pen == null)
					pen = new Pen ();
				pen.Width = ReadNumber (strokeWidth);
			}

			var strokeOpacity = GetString (style, "stroke-opacity");
			if (!string.IsNullOrWhiteSpace (strokeOpacity)) {
				if (pen == null)
					pen = new Pen ();
				pen.Color = pen.Color.WithAlpha (ReadNumber (strokeOpacity));
			}

			//
			// Pen
			//
			var stroke = GetString (style, "stroke").Trim ();
			if (string.IsNullOrEmpty (stroke)) {
				// No change
				hasPen = false;
			} else if (stroke.Equals("none", StringComparison.OrdinalIgnoreCase)) {
				hasPen = true;
				pen = null;
			} else {
				hasPen = true;
				if (pen == null)
					pen = new Pen ();
				Color color;
				if (Colors.TryParse (stroke, out color)) {
					if (pen.Color.Alpha == 1)
						pen.Color = color;
					else
						pen.Color = color.WithAlpha (pen.Color.Alpha);
				}
			}

			//
			// Brush attributes
			//
			var fillOpacity = GetString (style, "fill-opacity");
			if (!string.IsNullOrWhiteSpace (fillOpacity)) {
				if (brush == null)
					brush = new SolidBrush ();
				var sb = brush as SolidBrush;
				if (sb != null)
					sb.Color = sb.Color.WithAlpha (ReadNumber (fillOpacity));
			}

			//
			// Brush
			//
			var fill = GetString (style, "fill").Trim ();
			if (string.IsNullOrEmpty (fill)) {
				// No change
				hasBrush = false;
			} else if (fill.Equals("none", StringComparison.OrdinalIgnoreCase)) {
				hasBrush = true;
				brush = null;
			} else {
				hasBrush = true;
				Color color;
				if (Colors.TryParse (fill, out color)) {
					var sb = brush as SolidBrush;
					if (sb == null) {
						brush = new SolidBrush (color);
					} else {
						if (sb.Color.Alpha == 1)
							sb.Color = color;
						else
							sb.Color = color.WithAlpha (sb.Color.Alpha);
					}
				} else {
					var urlM = fillUrlRe.Match (fill);
					if (urlM.Success) {
						var id = urlM.Groups [1].Value.Trim ();
						XElement defE;
						if (defs.TryGetValue (id, out defE)) {
							switch (defE.Name.LocalName) {
							case "linearGradient":
								brush = CreateLinearGradientBrush (defE);
								break;
							case "radialGradient":
								brush = CreateRadialGradientBrush (defE);
								break;
							default:
								throw new NotSupportedException ("Fill " + defE.Name);
							}
						} else {
							throw new Exception ("Invalid fill url reference: " + id);
						}
					} else {
						throw new NotSupportedException ("Fill " + fill);
					}
				}
			}
		}

		Transform ReadTransform (string raw)
		{
			if (string.IsNullOrWhiteSpace (raw))
				return Transform.Identity;

			var s = raw.Trim ();

			var calls = s.Split (new[]{ ')' }, StringSplitOptions.RemoveEmptyEntries);

			var t = Transform.Identity;

			foreach (var c in calls) {
				var args = c.Split (new[]{ '(', ',', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				var nt = Transform.Identity;
				switch (args [0]) {
				case "matrix":
					if (args.Length == 7) {
						nt = new Transform (
							ReadNumber(args[1]), 
							ReadNumber(args[2]),
							ReadNumber(args[3]),
							ReadNumber(args[4]),
							ReadNumber(args[5]),
							ReadNumber(args[6]));
					} else {
						throw new NotSupportedException ("Matrices are expected to have 6 elements, this one has " + (args.Length - 1));
					}
					break;
				case "translate":
					if (args.Length >= 3) {
						nt = Transform.Translate (new Size (ReadNumber (args [1]), ReadNumber (args [2])));
					} else if (args.Length >= 2) {
						nt = Transform.Translate (new Size (ReadNumber (args[1]), 0));
					}
					break;
				case "scale":
					if (args.Length >= 3) {
						nt = Transform.Scale (new Size (ReadNumber (args[1]), ReadNumber (args[2])));
					} else if (args.Length >= 2) {
						var sx = ReadNumber (args [1]);
						nt = Transform.Scale (new Size (sx, sx));
					}
					break;
				case "rotate":
					var a = ReadNumber (args [1]);
					if (args.Length >= 4) {
						var x = ReadNumber (args [2]);
						var y = ReadNumber (args [3]);
						var t1 = Transform.Translate (new Size (x, y));
						var t2 = Transform.Rotate (a);
						var t3 = Transform.Translate (new Size (-x, -y));
						nt = t1 * t2 * t3;
					} else {
						nt = Transform.Rotate (a);
					}
					break;
				default:
					throw new NotSupportedException ("Can't transform " + args[0]);
				}
				t = t * nt;
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
				// Get the operation
				//
				var op = "";
				if (a.Length == 1) {
					op = a;
					i++;
				} else {
					op = a.Substring (0, 1);
					args [i] = a.Substring (1);
				}

				//
				// Execute
				//
				if (op == "M" && i + 1 < n) {
					p.MoveTo (new Point (ReadNumber (args [i]), ReadNumber (args [i + 1])));
					i += 2;
				} else if (op == "L" && i + 1 < n) {
					p.LineTo (new Point (ReadNumber (args [i]), ReadNumber (args [i + 1])));
					i += 2;
				} else if (op == "C" && i + 5 < n) {
					var c1 = new Point (ReadNumber (args [i]), ReadNumber (args [i + 1]));
					var c2 = new Point (ReadNumber (args [i + 2]), ReadNumber (args [i + 3]));
					var pt = new Point (ReadNumber (args [i + 4]), ReadNumber (args [i + 5]));
					p.CurveTo (c1, c2, pt);
					i += 6;
				} else if (op == "S" && i + 3 < n) {
					var c  = new Point (ReadNumber (args [i]), ReadNumber (args [i + 1]));
					var pt = new Point (ReadNumber (args [i + 2]), ReadNumber (args [i + 3]));
					p.ContinueCurveTo (c, pt);
					i += 4;
				} else if (op == "A" && i + 6 < n) {
					var r = new Size (ReadNumber (args [i]), ReadNumber (args [i + 1]));
//					var xr = ReadNumber (args [i + 2]);
					var laf = ReadNumber (args [i + 3]) != 0;
					var swf = ReadNumber (args [i + 4]) != 0;
					var pt = new Point (ReadNumber (args [i + 5]), ReadNumber (args [i + 6]));
					p.ArcTo (r, laf, swf, pt);
					i += 7;
				} else if (op == "z" || op == "Z") {
					p.Close ();
				} else {
					throw new NotSupportedException ("Path Operation " + op);
				}
			}
		}

		void ReadPolygon (Path p, string pathDescriptor)
		{
			var args = pathDescriptor.Split (new[]{' '}, StringSplitOptions.RemoveEmptyEntries);

			var i = 0;
			var n = args.Length;
			if (n == 0)
				throw new Exception ("Not supported polygon");
			while (i < n) {
				var x = ReadNumber (args [i]);
				var y = ReadNumber (args [i + 1]);

				if (i == 0) {
					p.MoveTo (x, y);
				} else
					p.LineTo (x, y);
				i += 2;

			}
			p.Close ();
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

		RadialGradientBrush CreateRadialGradientBrush (XElement e)
		{
			var b = new RadialGradientBrush ();

			b.Center.X = ReadNumber (e.Attribute ("cx"));
			b.Center.Y = ReadNumber (e.Attribute ("cy"));
			b.Focus.X = ReadNumber (e.Attribute ("fx"));
			b.Focus.Y = ReadNumber (e.Attribute ("fy"));
			var r = ReadNumber (e.Attribute ("r"));
			b.Radius = new Size (r);

			ReadStops (e, b.Stops);

			return b;
		}

		LinearGradientBrush CreateLinearGradientBrush (XElement e)
		{
			var b = new LinearGradientBrush ();

			b.Start.X = ReadNumber (e.Attribute ("x1"));
			b.Start.Y = ReadNumber (e.Attribute ("y1"));
			b.End.X = ReadNumber (e.Attribute ("x2"));
			b.End.Y = ReadNumber (e.Attribute ("y2"));

			ReadStops (e, b.Stops);

			return b;
		}

		void ReadStops (XElement e, List<GradientStop> stops)
		{
			var ns = e.Name.Namespace;
			foreach (var se in e.Elements (ns + "stop")) {
				var s = new GradientStop ();
				s.Offset = ReadNumber (se.Attribute ("offset"));
				var styleAttribute = se.Attribute("style");
				if (styleAttribute != null)
				{
					var styleSettings = styleAttribute.Value.Split(';');
					foreach(var style in styleSettings)
					{
						if (style.Contains("stop-color") && style.IndexOf(':') != -1)
						{
							s.Color = ReadColor(style.Substring(style.IndexOf(':')+1));
							break;
						}
					}
				}
				var stopColorAttribute = se.Attribute("stop-color");
				if (stopColorAttribute != null)
					s.Color = ReadColor (stopColorAttribute.Value);
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
			if (string.IsNullOrWhiteSpace (raw) || raw.Equals("none", StringComparison.OrdinalIgnoreCase))
				return Colors.Clear;

			var s = raw.Trim ();

			if (s.Length == 7 && s [0] == '#') 
			{

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
