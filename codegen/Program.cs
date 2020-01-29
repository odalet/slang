using System;
using System.IO;

namespace ILGen
{
    // See https://stackoverflow.com/questions/48090703/run-mono-cecil-in-net-core

    internal static class Program
    {

#if NETCOREAPP
        private const TargetFramework fx = TargetFramework.Core;
#else
        private const TargetFramework fx = TargetFramework.Fx;
#endif

        [STAThread]
        private static int Main()
        {
            Console.WriteLine($"Generating from {fx}...");
            try
            {
                var outputdir = @"C:\temp\ilgen";

                var ext = fx == TargetFramework.Fx ? "exe" : "dll";
                var baseName = fx.ToString().ToLowerInvariant();
                var output = Path.Combine(outputdir, $"{baseName}.{ext}");
                var generator = new Generator(fx, output);
                generator.Run();

                if (fx == TargetFramework.Core) File.Copy(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output_runtimeconfig.json"),
                    Path.Combine(outputdir, $"{baseName}.runtimeconfig.json"),
                    true);

                    Console.WriteLine("Done");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }
    }
}
