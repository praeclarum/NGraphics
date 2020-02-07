using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace NGraphics
{
	public class Graphic : IDrawable, IEdgeSampleable
	{
		public readonly List<Element> Children = new List<Element> ();

		public Size Size;
		public Rect ViewBox;
		public string Title = "";
		public string Description = "";

		public Graphic (Size size, Rect viewBox)
		{
			Size = size;
			ViewBox = viewBox;
		}

		public Graphic (Size size)
			: this (size, new Rect (Point.Zero, size))
		{
		}

		public Graphic ()
			: this (new Size ())
		{
		}

		public Graphic Clone ()
		{
			var g = new Graphic (Size, ViewBox) {
				Title = Title,
				Description = Description,
			};

			g.Children.AddRange (Children.Select (x => x.Clone ()));

			return g;
		}

		public Transform Transform {
			get {
				if (Size.Width > 0 && Size.Height > 0) {
					return Transform.StretchFillRect (ViewBox, new Rect (Point.Zero, Size));
				}
				else {
					return Transform.Identity;
				}
			}
		}

		public Graphic TransformGeometry (Transform transform)
		{
			var clone = Clone ();
			clone.Children.Clear ();
			clone.Children.AddRange (Children.Select (x => x.TransformGeometry (transform)));
			return clone;
		}

		public void Draw (ICanvas canvas)
		{
			canvas.SaveState ();

			//
			// Scale the viewBox into the size
			//
			canvas.Transform (Transform);

			//
			// Draw
			//
			foreach (var c in Children) {
				c.Draw (canvas);
			}

			canvas.RestoreState ();
		}

		public static Graphic LoadSvg (System.IO.TextReader reader, double pixelsPerInch = SvgReader.DefaultPixelsPerInch, Brush defaultBrush = null, Font defaultFont = null)
		{
            var svgr = new SvgReader (reader, pixelsPerInch: pixelsPerInch, defaultBrush: defaultBrush, defaultFont: defaultFont);
			return svgr.Graphic;
		}

		public static Graphic ParseSvg (string svg, double pixelsPerInch = SvgReader.DefaultPixelsPerInch, Brush defaultBrush = null, Font defaultFont = null)
		{			
			var svgr = new SvgReader (svg, pixelsPerInch: pixelsPerInch, defaultBrush: defaultBrush, defaultFont: defaultFont);
			return svgr.Graphic;
		}

		public void WriteSvg (System.IO.TextWriter writer)
		{
			var w = new SvgWriter (this, writer);
			w.Write ();
		}

		public string GetSvg ()
		{
			using (var tw = new StringWriterWithEncoding (Encoding.UTF8)) {
				WriteSvg (tw);
				return tw.ToString ();
			}
		}

		class StringWriterWithEncoding : StringWriter
		{
			readonly Encoding encoding;
			public StringWriterWithEncoding (Encoding encoding)
			{
				this.encoding = encoding;
			}
			public override Encoding Encoding => encoding;
		}

		public override string ToString ()
		{
			try {
				if (Children.Count == 0)
					return "Graphic";
				var w =
					Children.
					GroupBy (x => x.GetType ().Name).
					Select (x => x.Count () + " " + x.Key);
				return "Graphic with " + string.Join (", ", w);
			} catch {
				return "Graphic with errors!";
			}
		}

		public Element[] HitTest (Point worldPoint)
		{
			return Children.Where (x => x.HitTest (worldPoint)).Reverse().ToArray ();
		}

		#region ISampleable implementation

		public EdgeSamples[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples)
		{
			var r = new List<EdgeSamples> ();
			foreach (var c in Children.OfType<IEdgeSampleable> ()) {
				r.AddRange (c.GetEdgeSamples (tolerance, minSamples, maxSamples));
			}
			return r.ToArray ();
		}

		[System.Runtime.Serialization.IgnoreDataMember]
		public Rect SampleableBox {
			get {
				return ViewBox;
			}
		}

		#endregion
	}
}
