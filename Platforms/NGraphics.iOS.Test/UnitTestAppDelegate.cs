using System;
using System.Linq;
using System.Collections.Generic;

using Foundation;
using UIKit;
using NGraphics.Test;
using System.Threading.Tasks;

namespace NGraphics.iOS.Test
{
	[Register ("UnitTestAppDelegate")]
	public partial class UnitTestAppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window { get; set; }

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			Window = new UIWindow (UIScreen.MainScreen.Bounds);
			Window.RootViewController = new UIViewController ();

			System.Threading.ThreadPool.QueueUserWorkItem (async _ => {
				var tat = typeof (NUnit.Framework.TestAttribute);
				var tfat = typeof (NUnit.Framework.TestFixtureAttribute);

				var types = typeof (DrawingTest).Assembly.GetTypes ();
				var tfts = types.Where (t => t.GetCustomAttributes (tfat, false).Length > 0);

				PlatformTest.ResultsDirectory = System.IO.Path.Combine (Environment.GetEnvironmentVariable ("NGraphicsDir"), "TestResults");
				PlatformTest.Platform = Platforms.Current;
				Environment.CurrentDirectory = PlatformTest.ResultsDirectory;

				List<Exception> errors = new List<Exception> ();

				SetColor (UIColor.Yellow);

				foreach (var t in tfts) {
					var test = Activator.CreateInstance (t);
					var ms = t.GetMethods ().Where (m => m.GetCustomAttributes (tat, true).Length > 0);
					foreach (var m in ms) {
						Console.WriteLine ($"Running {m}");
						try {
							if (m.Invoke (test, null) is Task ttask) {
								await ttask;
							}
						}
						catch (Exception ex) {
							Console.WriteLine (ex);
							errors.Add (ex);
						}
					}
				}
				SetColor (errors.Count > 0 ? UIColor.Red : UIColor.Green);
			});

			Window.MakeKeyAndVisible ();

			return true;
		}

		void SetColor (UIColor color) {
			BeginInvokeOnMainThread (() => {
				Window.RootViewController.View.BackgroundColor = color;
			});
		}
	}
}

