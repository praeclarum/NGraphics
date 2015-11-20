namespace NGraphics.Net.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using NGraphics.Test;

    public class MainClass
    {
        public static void Main(string[] args)
        {
            RunTests().Wait();
        }

        private static async Task RunTests()
        {
            var sdir = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            while (Directory.GetFiles(sdir, "NGraphics.sln").Length == 0)
            {
                sdir = Path.GetDirectoryName(sdir);
            }

            PlatformTest.ResultsDirectory = Path.Combine(sdir, "TestResults");
            PlatformTest.Platform = Platforms.Current;
            Environment.CurrentDirectory = PlatformTest.ResultsDirectory;

            var tat = typeof(NUnit.Framework.TestAttribute);
            var tfat = typeof(NUnit.Framework.TestFixtureAttribute);

            var types = typeof(DrawingTest).Assembly.GetTypes();
            var tfts = types.Where(t => t.GetCustomAttributes(tfat, false).Length > 0);

            foreach (var t in tfts)
            {
                var test = Activator.CreateInstance(t);
                var ms = t.GetMethods().Where(m => m.GetCustomAttributes(tat, true).Length > 0);
                foreach (var m in ms)
                {
                    Console.WriteLine("Running {0}...", m);

                    try
                    {
                        var r = m.Invoke(test, null);
                        var ta = r as Task;
                        if (ta != null)
                        {
                            await ta;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.ReadLine();
                    }
                }
            }

            Console.WriteLine("Done");
        }
    }
}
