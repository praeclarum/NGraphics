using System;

using Foundation;
using AppKit;
using System.Text.RegularExpressions;

namespace NGraphics.Editor
{
	public partial class MainWindowController : NSWindowController
	{
		Style style = new Style ();

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

			Editor.AutomaticDashSubstitutionEnabled = false;
			Editor.AutomaticDataDetectionEnabled = false;
			Editor.AutomaticLinkDetectionEnabled = true;
			Editor.AutomaticQuoteSubstitutionEnabled = false;
			Editor.AutomaticSpellingCorrectionEnabled = false;
			Editor.AutomaticTextReplacementEnabled = false;

			Editor.Value = Code;
			HandleTextChanged ();
			HandleThrottledTextChanged ();
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
		string Code 
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

		static readonly Regex svgRe = new Regex (@"<svg.*?>.+<\/svg>", RegexOptions.Singleline);

		void ParseSVG()
		{
			SvgReader reader = new SvgReader(new System.IO.StringReader(Code));
			if (reader.Graphic != null)
			{
				this.BeginInvokeOnMainThread (() => {
					try {
						if (reader.Graphic.Size.Height == 0 || reader.Graphic.Size.Width == 0)
							reader.Graphic.Size = new Size(Prev.Bounds.Width, Prev.Bounds.Height);
						Prev.Drawables = new IDrawable[] { reader.Graphic };
						Prev.SetNeedsDisplayInRect (Prev.Bounds);
					} catch (Exception ex) {
						Console.WriteLine (ex);
					}
				});
			}
		}

		void HandleThrottledTextChanged ()
		{
			if (svgRe.IsMatch(Code))
				ParseSVG();
			else
				CompileCode ();
		}

		void CompileCode ()
		{
			// Done already?
			if (result != null && result.Code == Code) {
				return;
			}

			// Already requested?
			if (request != null) {
				if (request.Code == Code) {
					// The proper request is pending
					return;
				}
				request.Cancel (); // No need of this result
			}

			// Start a new request
			request = new CompileRequest (Code, AcceptCompileResult);
		}

		void AcceptCompileResult (CompileResult result)
		{
			this.BeginInvokeOnMainThread (() => {
				try {
					if (result.Code == Code) {
						Console.WriteLine ("NEW RESULT {0}", this.result);
						this.result = result;

						Errors.Value = result.Errors ?? "";
						Errors.TextColor = NSColor.Red;
					
						if (string.IsNullOrEmpty (result.Errors)) {
							Prev.Drawables = result.Drawables;
							Prev.SetNeedsDisplayInRect (Prev.Bounds);
						}
					}
				} catch (Exception ex) {
					Console.WriteLine (ex);
				}
			});
		}

		void HandleTextChanged ()
		{
			Console.WriteLine ("TEXT CHANGED");

			var s = Editor.TextStorage;
			Code = s.Value;

			s.BeginEditing ();
			style.FormatCode (s);
			s.EndEditing ();
		}

		class EditorDelegate : NSTextViewDelegate
		{
			public MainWindowController Controller;
			NSTimer changeThrottle = null;
			public override void TextDidChange (NSNotification notification)
			{
				try {
					Controller.HandleTextChanged ();
				} catch (Exception ex) {
					Console.WriteLine ();
				}
				if (changeThrottle != null) {
					changeThrottle.Invalidate ();
				}
				changeThrottle = NSTimer.CreateScheduledTimer (0.3333, t => {
					try {
						changeThrottle = null;
						Controller.HandleThrottledTextChanged ();
					} catch (Exception ex) {
						ShowError (ex);
					}	
				});

			}

			public override bool DoCommandBySelector (NSTextView textView, ObjCRuntime.Selector commandSelector)
			{
//				if (commandSelector.Name == "insertTab:") {
//					textView.InsertText (new NSString ("    "));
//					return true;
//				}
				return false;
			}
		}
	}
}
