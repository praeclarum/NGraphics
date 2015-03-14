using System;

using Foundation;
using AppKit;

namespace NGraphics.Editor
{
	public partial class MainWindowController : NSWindowController
	{
		public MainWindowController (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
		}

		public MainWindowController () : base ("MainWindow")
		{
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();

			Editor.Delegate = new EditorDelegate { Controller = this };

			Editor.Value = currentCode;
			CompileCode ();
		}

		public new MainWindow Window {
			get { return (MainWindow)base.Window; }
		}

		public static void ShowError (Exception error)
		{
			Console.WriteLine (error);
		}

		CompileResult result = null;
		CompileRequest request = null;
		string currentCode 
		{
			get {
				var d = Document as CSharpDocument;
				return d != null ? d.Code : "";
			}
			set {
				var d = Document as CSharpDocument;
				if (d != null) {
					d.Code = value;
				}
			}
		}

		void HandleTextChanged ()
		{
			Console.WriteLine ("TEXT CHANGED");

			currentCode = Editor.Value;

			CompileCode ();
		}

		void CompileCode ()
		{
			// Done already?
			if (result != null && result.Code == currentCode) {
				return;
			}

			// Already requested?
			if (request != null) {
				if (request.Code == currentCode) {
					// The proper request is pending
					return;
				}
				request.Cancel (); // No need of this result
			}

			// Start a new request
			request = new CompileRequest (currentCode, AcceptCompileResult);
		}

		void AcceptCompileResult (CompileResult result)
		{
			this.BeginInvokeOnMainThread (() => {
				try {
					if (result.Code == currentCode) {
						Console.WriteLine ("NEW RESULT {0}", this.result);
						this.result = result;
					
						Prev.Drawables = result.Drawables;
						Prev.SetNeedsDisplayInRect (Prev.Bounds);
					}
				} catch (Exception ex) {
					Console.WriteLine (ex);
				}
			});
		}

		class EditorDelegate : NSTextViewDelegate
		{
			public MainWindowController Controller;
			NSTimer changeThrottle = null;
			public override void TextDidChange (NSNotification notification)
			{
				if (changeThrottle != null) {
					changeThrottle.Invalidate ();
				}
				changeThrottle = NSTimer.CreateScheduledTimer (0.3333, t => {
					try {
						changeThrottle = null;
						Controller.HandleTextChanged ();
					} catch (Exception ex) {
						ShowError (ex);
					}	
				});

			}
		}
	}
}
