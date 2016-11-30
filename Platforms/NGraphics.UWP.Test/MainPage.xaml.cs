using NGraphics.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Reflection;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NGraphics.UWP.Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded (object sender, RoutedEventArgs e)
        {
            await RunUnitTests ();
        }

        class FileMemoryStream : MemoryStream
        {
            public string Path;
        }

        async Task RunUnitTests ()
        {
            statusText.Text = $"Running unit tests.";
            var timer = new Stopwatch();
            timer.Start();

            var tat = typeof (NUnit.Framework.TestAttribute);
            var tfat = typeof (NUnit.Framework.TestFixtureAttribute);

            var types = typeof (DrawingTest).GetTypeInfo ().Assembly.ExportedTypes;
            var tfts = types.Where (t => t.GetTypeInfo ().GetCustomAttributes (tfat, false).Any ());

            var ngd = "";
            PlatformTest.ResultsDirectory = System.IO.Path.Combine (ngd, "TestResults");
            PlatformTest.Platform = Platforms.Current;

            var client = new HttpClient ();

            PlatformTest.OpenStream = path => {
                return new FileMemoryStream { Path = path, };
            };
            //PlatformTest.CloseStream = async stream => {
            //	var path = ((FileMemoryStream)stream).Path;
            //	var url = "http://192.168.1.100:1234/" + System.IO.Path.GetFileName (path);
            //	System.Diagnostics.Debug.WriteLine ("POSTING " + url);
            //	stream.Position = 0;
            //	var content = new StreamContent (stream);
            //             await client.PostAsync (url, content);
            //};

            // Save test results to LocalState folder
            PlatformTest.CloseStream = async stream => {
                var path = ((FileMemoryStream)stream).Path;
                System.Diagnostics.Debug.WriteLine ("SAVING " + path);
                statusText.Text += $"{Environment.NewLine}Saving: {path}"; // show in UI also
                stream.Position = 0;
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                var file = await localFolder.CreateFileAsync (path, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                using (var fileStream = await file.OpenStreamForWriteAsync ())
                    await stream.CopyToAsync (fileStream);
            };

            foreach (var t in tfts) {
				var test = Activator.CreateInstance (t);
				var ms = t.GetRuntimeMethods ().Where (m => m.GetCustomAttributes (tat, true).Any ());
				foreach (var m in ms) {
					try {
						var r = m.Invoke (test, null);
						var ta = r as Task;
						if (ta != null)
							await ta;
					}
					catch (Exception ex) {
						System.Diagnostics.Debug.WriteLine (ex);
					}
				}
			}

            timer.Stop();
            statusText.Text += $"{Environment.NewLine}";
            statusText.Text += $"{Environment.NewLine}Done! Elapsed: {timer.ElapsedMilliseconds}ms";
            statusText.Text += $"{Environment.NewLine}Images saved here: {System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, PlatformTest.ResultsDirectory) .ToString ()}";
        }
    }
}
