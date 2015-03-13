// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace NGraphics.Editor
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		AppKit.NSTextView Editor { get; set; }

		[Outlet]
		AppKit.NSTextView Errors { get; set; }

		[Outlet]
		NGraphics.Editor.Preview Prev { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (Editor != null) {
				Editor.Dispose ();
				Editor = null;
			}

			if (Prev != null) {
				Prev.Dispose ();
				Prev = null;
			}

			if (Errors != null) {
				Errors.Dispose ();
				Errors = null;
			}
		}
	}
}
