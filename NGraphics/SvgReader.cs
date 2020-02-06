using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NGraphics
{
	public class SvgReader
	{
		readonly IFormatProvider icult = System.Globalization.CultureInfo.InvariantCulture;

		public double PixelsPerInch { get; private set; }
		public Graphic Graphic { get; private set; }

		public List<Exception> Errors { get; } = new List<Exception> ();

		readonly Dictionary<string, XElement> defs = new Dictionary<string, XElement> ();
		//		readonly XNamespace ns;

		static readonly XmlReaderSettings readerSettings = new XmlReaderSettings {
			DtdProcessing = DtdProcessing.Ignore,
		};

		public const double DefaultPixelsPerInch = 160.0;

		public SvgReader (System.IO.TextReader textReader, double pixelsPerInch = DefaultPixelsPerInch, Brush defaultBrush = null)
			: this (XmlReader.Create (textReader, readerSettings), pixelsPerInch, defaultBrush)
		{
		}

		//public SvgReader (System.IO.Stream stream, double pixelsPerInch = DefaultPixelsPerInch, Brush defaultBrush = null)
		//	: this (XmlReader.Create (stream, readerSettings), pixelsPerInch, defaultBrush)
		//{
		//}

		SvgReader (System.Xml.XmlReader xmlReader, double pixelsPerInch = DefaultPixelsPerInch, Brush defaultBrush = null)
		{
			defaultBrush = defaultBrush ?? Brushes.Black;
			PixelsPerInch = pixelsPerInch;
			Read (XDocument.Load (xmlReader), defaultBrush);
		}

		public SvgReader (string svgString, double pixelsPerInch = 160.0, Brush defaultBrush = null)
		{
			PixelsPerInch = pixelsPerInch;
			Read (XDocument.Parse(svgString), defaultBrush);
		}

		void Read (XDocument doc, Brush defaultBrush)
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

			if (heightA == null && widthA == null && viewBoxA != null) {
				size = new Size(viewBox.Width, viewBox.Height);
			}

			//
			// Add the elements
			//
			Graphic = new Graphic (size, viewBox);

			AddElements (Graphic.Children, svg.Elements (), null, defaultBrush);
		}

		void AddElements (IList<Element> list, IEnumerable<XElement> es, Pen inheritPen, Brush inheritBrush)
		{
			foreach (var e in es)
				AddElement (list, e, inheritPen, inheritBrush);
		}

		void GetPenAndBrush (XElement e, Pen inheritPen, Brush inheritBrush, out Pen pen, out Brush brush)
		{
			bool hasPen, hasBrush;
			pen = null;
			brush = null;
			ApplyStyle (e.Attributes().ToDictionary(k => k.Name.LocalName, v => v.Value), ref pen, out hasPen, ref brush, out hasBrush);
			var style = ReadString (e.Attribute("style"));
			if (!string.IsNullOrWhiteSpace(style)) {
				ApplyStyle(style, ref pen, out hasPen, ref brush, out hasBrush);
			}
			pen = hasPen ? pen : inheritPen;
			brush = hasBrush ? brush : inheritBrush;
		}

		void AddElement (IList<Element> list, XElement e, Pen inheritPen, Brush inheritBrush)
		{
			//
			// Style
			//
			Element r = null;
			GetPenAndBrush (e, inheritPen, inheritBrush, out var pen, out var brush);
			//var id = ReadString (e.Attribute ("id"));

			//
			// Elements
			//
			switch (e.Name.LocalName) {
			case "text":
				{
					var x = ReadNumber (e.Attribute ("x"));
					var y = ReadNumber (e.Attribute ("y"));
					var font = new Font ();
					var fontFamily = ReadTextFontFamily(e);
					if (!string.IsNullOrEmpty(fontFamily))
						font.Family = fontFamily;
					var fontSize = ReadTextFontSize(e);
					if (fontSize >= 0)
						font.Size = fontSize;
					TextAlignment textAlignment = ReadTextAlignment(e);
					var txt = new Text (new Rect (new Point (x, y), new Size (double.MaxValue, double.MaxValue)), font, textAlignment, pen, brush);
					ReadTextSpans (txt, e, pen, brush);
					r = txt;
				}
				break;
			case "rect":
				{
					var x = ReadNumber (e.Attribute ("x"));
					var y = ReadNumber (e.Attribute ("y"));
					var width = ReadNumber (e.Attribute ("width"));
					var height = ReadNumber (e.Attribute ("height"));
					var rx = ReadNumber (e.Attribute ("rx"));
					var ry = ReadNumber (e.Attribute ("ry"));
					if (ry == 0) {
						ry = rx;
					}
					r = new Rectangle (new Rect (new Point (x, y), new Size (width, height)), new Size (rx, ry), pen, brush);
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
						try {
							ReadPath (p, dA.Value);
						}
						catch (Exception ex) {
							Errors.Add (ex);
						}
						r = p;
					}
				}
				break;
			case "polygon":
				{
					var pA = e.Attribute ("points");
					if (pA != null && !string.IsNullOrWhiteSpace (pA.Value)) {
						var path = new Path (pen, brush);
						ReadPoints (path, pA.Value, true);
						r = path;
					}
				}
				break;
			case "polyline":
				{
					var pA = e.Attribute ("points");
					if (pA != null && !string.IsNullOrWhiteSpace (pA.Value)) {
						var path = new Path (pen, brush);
						ReadPoints (path, pA.Value, false);
						r = path;
					}
				}
			break;
			case "g":
				{
					var g = new Group ();
					var groupId = e.Attribute("id");
					if (groupId != null && !string.IsNullOrEmpty(groupId.Value))
						g.Id = groupId.Value;

					var groupOpacity = e.Attribute ("opacity");
					if (groupOpacity != null && !string.IsNullOrEmpty (groupOpacity.Value)) 
						g.Opacity = ReadNumber (groupOpacity);

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
							var useList = new List<Element> ();
							AddElement (useList, useE, pen, brush);
							r = useList.FirstOrDefault ();
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
					var p = new Path (pen, null);
					p.MoveTo (x1, y1);
					p.LineTo (x2, y2);
					r = p;
				}
				break;

				case "foreignObject":
				{
					var x = ReadNumber ( e.Attribute("x") );
					var y = ReadNumber ( e.Attribute("y") );
					var width = ReadNumber ( e.Attribute("width") );
					var height = ReadNumber ( e.Attribute("height") );
					r = new ForeignObject(new Point(x, y), new Size(width, height));
				}
				break;

				case "pgf":
				{
					var id = e.Attribute("id");
					System.Diagnostics.Debug.WriteLine("Ignoring pgf element" + (id != null ? ": '" + id.Value + "'" : ""));
				}
				break;

				case "switch":
				{
					// Evaluate requiredFeatures, requiredExtensions and systemLanguage
					foreach (var ee in e.Elements())
					{
						var requiredFeatures = ee.Attribute("requiredFeatures");
						var requiredExtensions = ee.Attribute("requiredExtensions");
						var systemLanguage = ee.Attribute("systemLanguage");
						// currently no support for any of these restrictions
						if (requiredFeatures == null && requiredExtensions == null && systemLanguage == null)
							AddElement (list, ee, pen, brush);
					}
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
				var ida = e.Attribute("id");
				if (ida != null && !string.IsNullOrEmpty (ida.Value)) {
					r.Id = ida.Value.Trim ();
				}
				list.Add (r);
			}
		}

		Regex keyValueRe = new Regex (@"\s*([\w-]+)\s*:\s*(.*)");

		void ApplyStyle (string style, ref Pen pen, out bool hasPen, ref Brush brush, out bool hasBrush)
		{
			var d = ParseStyle(style);
			ApplyStyle (d, ref pen, out hasPen, ref brush, out hasBrush);
		}

		Dictionary<string, string> ParseStyle(string style)
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
			return d;
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
			if (string.IsNullOrWhiteSpace (strokeOpacity))
				strokeOpacity = GetString (style, "opacity");
			
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
			if (string.IsNullOrWhiteSpace (fillOpacity))
				fillOpacity = GetString (style, "opacity");
			
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
						brush = GetGradientBrush(id, null);
					} else {
						throw new NotSupportedException ("Fill " + fill);
					}
				}
			}
		}

		protected GradientBrush GetGradientBrush(string fill, GradientBrush child)
		{
			XElement defE;
			if (defs.TryGetValue (fill, out defE)) {
				GradientBrush brush = null;
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
				if (child != null)
				{	
					if (child is RadialGradientBrush && brush is RadialGradientBrush)
					{
						((RadialGradientBrush)brush).Center = ((RadialGradientBrush)child).Center;
						((RadialGradientBrush)brush).Focus = ((RadialGradientBrush)child).Focus;
						((RadialGradientBrush)brush).Radius = ((RadialGradientBrush)child).Radius;
					} else if (child is LinearGradientBrush && brush is LinearGradientBrush)
					{
						((LinearGradientBrush)brush).Start = ((LinearGradientBrush)child).Start;
						((LinearGradientBrush)brush).End = ((LinearGradientBrush)child).End;
					}

					brush.AddStops(child.Stops);
				}

				XNamespace xlink = "http://www.w3.org/1999/xlink";
				var parent = defE.Attribute(xlink + "href");
				if (parent != null && !string.IsNullOrEmpty(parent.Value))
				{
					brush = GetGradientBrush(parent.Value.Substring(1), brush);
				}
				return brush;
			} else {
				throw new Exception ("Invalid fill url reference: " + fill);
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

		void ReadTextSpans (Text txt, XElement e, Pen inheritPen, Brush inheritBrush)
		{
			foreach (XNode c in e.Nodes ()) {
				if (c.NodeType == XmlNodeType.Text) {
					txt.Spans.Add (new TextSpan (((XText)c).Value) {
						Pen = inheritPen,
						Brush = inheritBrush
					});
				} else if (c.NodeType == XmlNodeType.Element) {
					var ce = (XElement)c;
					if (ce.Name.LocalName == "tspan") {
						var tspan = new TextSpan (ce.Value);
						GetPenAndBrush (ce, inheritPen, inheritBrush, out tspan.Pen, out tspan.Brush);
						var x = ReadOptionalNumber (ce.Attribute ("x"));
						var y = ReadOptionalNumber (ce.Attribute ("y"));
						if (x.HasValue && y.HasValue) {
							tspan.Position = new Point (x.Value, y.Value);
						}

						var font = txt.Font;

						var ffamily = ReadTextFontFamily (ce);
						if (!string.IsNullOrWhiteSpace (ffamily)) {
							font = font.WithFamily (ffamily);
						}
						var fweight = ReadTextFontWeight (ce);
						if (!string.IsNullOrWhiteSpace (fweight)) {
							font = font.WithWeight (fweight);
						}
						var fstyle = ReadTextFontStyle (ce);
						if (!string.IsNullOrWhiteSpace (fstyle)) {
							font = font.WithStyle (fstyle);
						}
						var fsize = ReadTextFontSize (ce);
						if (fsize > 0) {
							font = font.WithSize (fsize);
						}

						if (font != txt.Font) {
							tspan.Font = font;
						}

						txt.Spans.Add (tspan);
					}
				}
			}
			txt.Trim ();
		}

		static readonly char[] WSC = new char[] { ',', ' ', '\t', '\n', '\r' };

		static readonly Regex pathRegex = new Regex(@"[MLHVCSQTAZmlhvcsqtaz][^MLHVCSQTAZmlhvcsqtaz]*", RegexOptions.Singleline);
		static readonly Regex negativeNumberRe = new Regex("(?<=[0-9])-");
		static readonly Regex floatingPointRe = new Regex ("(?<=\\.[0-9]+)\\.");

		struct PathToken
		{
			public double Value;
			public bool IsNumber;
			public char Operator;
			public override string ToString () => IsNumber ? Value.ToString ("#.00") : Operator.ToString ();
		}

		List<PathToken> LexPath (string p)
		{
			var i = 0;
			var n = p.Length;
			var tokens = new List<PathToken> ();
			while (i < n) {
				while (i < n && char.IsWhiteSpace (p[i]))
					i++;
				if (i >= n)
					break;
				switch (p[i]) {
					case '.':
					case '-':
					case '+':
					case var d when char.IsDigit (d): {
							var gotDot = false;
							var gotE = 0;
							var s = i;
							i++;
							while (i < n && (
								char.IsDigit (p[i]) ||
								(!gotDot && p[i] == '.') ||
								(gotE == 0 && char.ToLowerInvariant (p[i]) == 'e') ||
								(gotE == i - 1 && (p[i] == '+' || p[i] == '-')))) {
								gotDot = p[i] == '.';
								if (char.ToLowerInvariant (p[i]) == 'e')
									gotE = i;
								i++;
							}
							var str = p.Substring (s, i - s);
							tokens.Add (new PathToken { IsNumber = true, Value = ReadNumber (str) });
						}
						break;
					case ',':
						i++;
						break;
					default:
						tokens.Add (new PathToken { Operator = p[i] });
						i++;
						break;
				}
			}
			return tokens;
		}

		void ReadPath (Path p, string pathDescriptor)
		{
            Point previousPoint = new Point();
			var tokens = LexPath (pathDescriptor);
			var ntokens = tokens.Count;
			var it = 0;
			var argsLength = 0;
			var argsStartIndex = 0;
			double Arg (int index) => tokens[argsStartIndex + index].Value;

			while (it < ntokens)
			{
				while (it < ntokens && tokens[it].IsNumber)
					it++;
				if (it >= ntokens)
					break;

				var op = tokens[it].Operator;

				var itEnd = it + 1;
				while (itEnd < ntokens && tokens[itEnd].IsNumber)
					itEnd++;
				argsStartIndex = it + 1;
				argsLength = itEnd - argsStartIndex;

				if (op == 'z' || op == 'Z') {
					p.Close ();
				}
				else if (op == 'M' || op == 'm') {
					var i = 0;
					while (i + 1 < argsLength) {
						var point = new Point (Arg (i), Arg (i + 1));
						if (op == 'm') {
							point += previousPoint;
						}
						if (i == 0) {
							p.MoveTo (point);
						}
						else {
							p.LineTo (point);
						}
						previousPoint = point;
						i += 2;
					}
				}
				else if (op == 'L' || op == 'l') {
					var i = 0;
					while (i + 1 < argsLength) {
						var point = new Point (Arg (i), Arg (i + 1));
						if (op == 'l') {
							point += previousPoint;
						}
						p.LineTo (point);
						previousPoint = point;
						i += 2;
					}
				}
				else if (op == 'C' || op == 'c') {
					var i = 0;
					while (i + 5 < argsLength) {
						var c1 = new Point (Arg (i + 0), Arg (i + 1));
						var c2 = new Point (Arg (i + 2), Arg (i + 3));
						var pt = new Point (Arg (i + 4), Arg (i + 5));
						if (op == 'c') {
							c1 += previousPoint;
							c2 += previousPoint;
							pt += previousPoint;
						}
						p.CurveTo (c1, c2, pt);
						previousPoint = pt;
						i += 6;
					}
				}
				else if (op == 'S' || op == 's') {
					var i = 0;
					while (i + 3 < argsLength) {
						var c = new Point (Arg (i + 0), Arg (i + 1));
						var pt = new Point (Arg (i + 2), Arg (i + 3));
						if (op == 's') {
							c += previousPoint;
							pt += previousPoint;
						}
						p.ContinueCurveTo (c, pt);
						previousPoint = pt;
						i += 4;
					}
				}
				else if (op == 'A' || op == 'a') {
					var i = 0;
					while (i + 6 < argsLength) {
						var r = new Size (Arg (i + 0), Arg (i + 1));
						//var xr = Arg (i + 2);
						var laf = Arg (i + 3) != 0;
						var swf = Arg (i + 4) != 0;
						var pt = new Point (Arg (i + 5), Arg (i + 6));
						if (op == 'a') {
							pt += previousPoint;
						}
						p.ArcTo (r, laf, swf, pt);
						previousPoint = pt;
						i += 7;
					}
				}
				else if (op == 'V' || op == 'v') {
					var i = 0;
					while (i < argsLength) {
						var previousX = previousPoint.X;
						var y = Arg (i);
						if (op == 'v') {
							y += previousPoint.Y;
						}
						var pt = new Point (previousX, y);
						p.LineTo (pt);
						previousPoint = pt;
						i += 1;
					}
				}
				else if (op == 'H' || op == 'h') {
					var i = 0;
					while (i < argsLength) {
						var previousY = previousPoint.Y;
						var x = Arg (i);
						if (op == 'h')
							x += previousPoint.X;
						var pt = new Point (x, previousY);
						p.LineTo (pt);
						previousPoint = pt;
						i += 1;
					}
				}
				else {
					throw new NotSupportedException ("Path Operation " + op);
				}

				it = itEnd;
			}
			
		}

		void ReadPoints (Path p, string pathDescriptor, bool closePath)
		{
			var args = pathDescriptor.Split (new[]{' '}, StringSplitOptions.RemoveEmptyEntries);

			var i = 0;
			var n = args.Length;
			if (n == 0)
				throw new Exception ("No points specified");
			while (i < n) {
				var xy = args[i].Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries);
				var x = 0.0;
				var y = 0.0;
				var di = 1;
				if (xy.Length == 1) {
					x = ReadNumber (args [i]);
					y = ReadNumber (args [i + 1]);
					di = 2;
				}
				else {
					x = ReadNumber (xy[0]);
					y = ReadNumber (xy[1]);
				}

				if (i == 0) {
					p.MoveTo (x, y);
				} else {
					p.LineTo (x, y);
				}
				i += di;
			}
			if (closePath)
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
			if (e.Attribute ("fx") != null)
				b.Focus.X = ReadNumber (e.Attribute ("fx"));
			else
				b.Focus.X = b.Center.X;
			if (e.Attribute ("fy") != null)
				b.Focus.Y = ReadNumber (e.Attribute ("fy"));
			else
				b.Focus.Y = b.Center.Y;
			var r = ReadNumber (e.Attribute ("r"));
			b.Radius = new Size (r);

			var gradientUnits = e.Attribute("gradientUnits");
			if (gradientUnits != null)
			{
				b.Absolute = gradientUnits.Value == "userSpaceOnUse";
			}

			// TODO: check gradientTransform attribute

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

			var gradientUnits = e.Attribute("gradientUnits");
			if (gradientUnits != null)
			{
				b.Absolute = gradientUnits.Value == "userSpaceOnUse";
			}

			// TODO: check gradientTransform attribute

			ReadStops (e, b.Stops);

			return b;
		}

		void ReadStops (XElement e, List<GradientStop> stops)
		{
			var ns = e.Name.Namespace;
			foreach (var se in e.Elements (ns + "stop")) {
				var s = new GradientStop ();
				s.Offset = ReadNumber (se.Attribute ("offset"));
				double alpha = 1.0;
				var styleAttribute = se.Attribute("style");
				if (styleAttribute != null)
				{
					var styleSettings = styleAttribute.Value.Split(';');
					foreach(var style in styleSettings)
					{
						if (style.Contains("stop-color") && style.IndexOf(':') != -1)
						{
							s.Color = ReadColor(style.Substring(style.IndexOf(':')+1));
						}
						else if (style.Contains("stop-opacity") && style.IndexOf(':') != -1)
						{
							alpha = ReadNumber(style.Substring(style.IndexOf(':')+1));
						}
					}
				}
				var stopColorAttribute = se.Attribute("stop-color");
				if (stopColorAttribute != null)
					s.Color = ReadColor (stopColorAttribute.Value);
				var opacityAttribute = se.Attribute("stop-opacity");
				if (opacityAttribute != null)
					alpha = ReadNumber(opacityAttribute.Value);
				s.Color.Alpha = alpha;
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

		Regex rgbRe = new Regex("([0-9]+).*?([0-9]+).*?([0-9]+)");

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

			var match = rgbRe.Match(s);
			if (match.Success && match.Groups.Count == 4)
			{
				var r = int.Parse( match.Groups[1].Value );
				var g = int.Parse( match.Groups[2].Value );
				var b = int.Parse( match.Groups[3].Value );

				return new Color (r / 255.0, g / 255.0, b / 255.0, 1);
			}

			Color color;
			if (Colors.TryParse (s, out color))
				return color;

			throw new NotSupportedException ("Color " + s);
		}

		string ReadTextFontAttr (XElement element, string attr)
		{
			string value = null;
			if (element != null)
			{
				var attrib = element.Attribute(attr);
				if (attrib != null && !string.IsNullOrWhiteSpace(attrib.Value))
					value = attrib.Value.Trim();
				else
				{
					var style = element.Attribute("style");
					if (style != null && !string.IsNullOrWhiteSpace(style.Value))
					{
						value = GetString(ParseStyle(style.Value), "attr");
					}
				}
			}
			return value;

		}

		string ReadTextFontFamily (XElement element)
		{
			return ReadTextFontAttr (element, "font-family");
		}

		string ReadTextFontWeight (XElement element)
		{
			return ReadTextFontAttr (element, "font-weight");
		}

		string ReadTextFontStyle (XElement element)
		{
			return ReadTextFontAttr (element, "font-style");
		}

		double ReadTextFontSize(XElement element)
		{
			double value = -1;
			if (element != null)
			{
				var attrib = element.Attribute("font-size");
				if (attrib != null && !string.IsNullOrWhiteSpace(attrib.Value))
					value = ReadNumber(attrib.Value);
				else
				{
					var style = element.Attribute("style");
					if (style != null && !string.IsNullOrWhiteSpace(style.Value))
					{
						value = ReadNumber(GetString(ParseStyle(style.Value), "font-size", "-1"));
					}
				}
			}

			return value;
		}

		TextAlignment ReadTextAlignment(XElement element)
		{
			string value = null;
			if (element != null)
			{
				var attrib = element.Attribute("text-anchor");
				if (attrib != null && !string.IsNullOrWhiteSpace(attrib.Value))
					value = attrib.Value;
				else
				{
					var style = element.Attribute("style");
					if (style != null && !string.IsNullOrWhiteSpace(style.Value))
					{
						value = GetString (ParseStyle(style.Value), "text-anchor");
					}
				}
			}

			switch (value) {
			case "end":
				return TextAlignment.Right;
			case "middle":
				return TextAlignment.Center;
			default:
				return TextAlignment.Left;
			}
		}

		double ReadNumber (XAttribute a)
		{
			if (a == null)
				return 0;
			return ReadNumber (a.Value);
		}

		double? ReadOptionalNumber (XAttribute a)
		{
			if (a == null)
				return null;
			return ReadNumber (a.Value);
		}

		Regex unitRe = new Regex("px|pt|em|ex|pc|cm|mm|in");
		Regex percRe = new Regex("%");

		double ReadNumber (string raw)
		{
			if (string.IsNullOrWhiteSpace (raw))
				return 0;

			var s = raw.Trim ();
			var m = 1.0;

			if (unitRe.IsMatch(s)) {
				if (s.EndsWith ("in", StringComparison.Ordinal)) {
					m = PixelsPerInch;
				} else if (s.EndsWith ("cm", StringComparison.Ordinal)) {
					m = PixelsPerInch / 2.54;
				} else if (s.EndsWith ("mm", StringComparison.Ordinal)) {
					m = PixelsPerInch / 25.4;
				} else if (s.EndsWith ("pt", StringComparison.Ordinal)) {
					m = PixelsPerInch / 72.0;
				} else if (s.EndsWith ("pc", StringComparison.Ordinal)) {
					m = PixelsPerInch / 6.0;
				}
				s = s.Substring (0, s.Length - 2);
			} else if (percRe.IsMatch(s)) {
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
