using System;
using Delta.Slang.Backend.IL;

namespace ilcore
{
    class Program
    {
        static void Main(string[] args)
        {
#if NETCOREAPP
            var name = "netcore";
#else
            var name = "net472";
#endif

            try
            {
                var builder = new Builder($@"c:\temp\{name}.exe");
                builder.Build();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
