using System;
using System.IO;

namespace TestApp
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            //var rc = new Program().Run(args);
            var rc = 0;
            Foo1.Test();

#if DEBUG
            Console.ReadKey();
#endif
            return rc;
        }

        private int Run(string[] args)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "hello.sl");
            using (var reader = new StreamReader(File.OpenRead(path)))
            {
                while (!reader.EndOfStream)
                {
                    var c1 = (char)reader.Peek();
                    var c = (char)reader.Read();
                }
            }

            return 0;
        }
    }
}
