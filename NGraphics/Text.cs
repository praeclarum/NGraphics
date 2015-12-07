using System;
using System.Globalization;
using System.Collections.Generic;

namespace NGraphics
{
	public class Text : Element
	{
		public Rect Frame;
		public string String;
		public TextAlignment Alignment;
		public Font Font;

		public Text (string text, Rect frame, Font font, TextAlignment alignment = TextAlignment.Left, Pen pen = null, Brush brush = null)
			: base (pen, brush)
		{
			String = text;
			Frame = frame;
			Font = font;
			Alignment = alignment;
		}

		protected override void DrawElement (ICanvas canvas)
		{
			canvas.DrawText (String, Frame, Font, Alignment, Pen, Brush);
		}

		#region implemented abstract members of Element

		public override EdgeSamples[] GetEdgeSamples (double tolerance, int minSamples, int maxSamples)
		{
			return new EdgeSamples[0];
		}

		public override Rect SampleableBox {
			get {
				return Frame;
			}
		}

		#endregion
	}
}
