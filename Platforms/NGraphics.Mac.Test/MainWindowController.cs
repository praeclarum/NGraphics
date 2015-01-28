using System;

using Foundation;
using AppKit;
using System.Linq;

namespace NGraphics.Mac.Test
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

			var tat = typeof(NUnit.Framework.TestAttribute);
			var tfat = typeof(NUnit.Framework.TestFixtureAttribute);

			var types = System.Reflection.Assembly.GetExecutingAssembly ().GetTypes ();
			var tfts = types.Where (t => t.GetCustomAttributes (tfat, false).Length > 0);

			foreach (var t in tfts) {
				var test = Activator.CreateInstance (t);
				var ms = t.GetMethods ().Where (m => m.GetCustomAttributes (tat, true).Length > 0);
				foreach (var m in ms) {
					m.Invoke (test, null);
				}
			}
		}

		public new MainWindow Window {
			get { return (MainWindow)base.Window; }
		}
	}
}
