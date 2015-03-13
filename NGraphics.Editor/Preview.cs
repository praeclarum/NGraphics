using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace NGraphics.Editor
{
	public partial class Preview : AppKit.NSView
	{
		#region Constructors

		// Called when created from unmanaged code
		public Preview (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public Preview (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public IDrawable[] Drawables;

		LinearGradientBrush backBrush = new LinearGradientBrush (
            Point.Zero, Point.OneY,
			new Color (0.97),
			new Color (0.90));

		public override void DrawRect (CoreGraphics.CGRect dirtyRect)
		{
			base.DrawRect (dirtyRect);

			var canvas = new NGraphics.CGContextCanvas (NSGraphicsContext.CurrentContext.CGContext);

			canvas.FillRectangle (Conversions.GetRect (this.Bounds), backBrush);

			var ds = Drawables;
			if (ds == null || ds.Length == 0)
				return;

			foreach (var d in ds) {
				try {
					d.Draw (canvas);
				} catch (Exception ex) {
					Console.WriteLine (ex);
				}
			}
		}
	}
}
