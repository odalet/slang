using System;
using System.IO;

namespace Scratch
{
    // This serves as a scratch buffer to test things

    internal class Program
    {
        static void Main()
        {
            new TestFormatter().Test();
            Console.WriteLine("---------------------------");

            Test1();
            Test2();
            Test3();
            Test4();
            Test5();
            Test6();
            Console.WriteLine("DONE");
        }

        private static void Test1()
        {
            var ok = char.IsDigit('٣');
            Console.WriteLine(ok);
        }

        private static void Test2()
        {
            Console.WriteLine(-12.Abs());
            Console.WriteLine(-3.14.Abs());

            Console.WriteLine((-12).Abs());
            Console.WriteLine((-3.14).Abs());
        }

        private static void Test3()
        {
            File.WriteAllText(@"c:\temp\crlf.txt", "A\rB\r\nC\nD\r\rE\r\r\n");
        }

        private static void Test4()
        {
            var i = +-+-+-+-+-+-+-+-1; // Yeah, this works!
            var j = 09;
            Console.WriteLine($"i = {i}, j = {j}");
        }

        private static void Test5()
        {
            var s = "Hello";
            var a = s + 1;
            var b = 1 + s;

            Console.WriteLine(a);
            Console.WriteLine(b);
        }

        private static void Test6()
        {
            var i = 42;
            Console.WriteLine((double)i);

            object j = i;
            Console.WriteLine((double)(int)j);
        }
    }

    static class Ext
    {
        public static int Abs(this int i) => Math.Abs(i);
        public static double Abs(this double i) => Math.Abs(i);
    }
}