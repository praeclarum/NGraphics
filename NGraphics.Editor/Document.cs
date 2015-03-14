using System;
using AppKit;
using Foundation;

namespace NGraphics.Editor
{
	[Register("CSharpDocument")]
	public class CSharpDocument : NSDocument
	{
		public string Code { get; set; }

		public CSharpDocument (IntPtr handle)
			: base (handle)
		{
			Initialize ();
		}

		void Initialize ()
		{
			Code = @"using System;
using System.Collections.Generic;
using System.Linq;
using NGraphics; 
class Drawing : IDrawable
{
	public void Draw (ICanvas canvas)
	{
		canvas.FillEllipse (new Rect(100, 100, 200, 200), Color.FromRGB (0x6495ED));
	}
}
";
		}

		[Export ("autosavesInPlace")]
		static bool GetAutosavesInPlace () { return true; }

		public override void MakeWindowControllers ()
		{
			var wc = new MainWindowController ();
			AddWindowController (wc);
		}

		public override NSData GetAsData (string typeName, out NSError outError)
		{
			outError = null;
			return NSData.FromString (Code);
		}

		public override bool ReadFromData (NSData data, string typeName, out NSError outError)
		{
			outError = null;
			Code = data.ToString ();
			return true;
		}
	}
}

