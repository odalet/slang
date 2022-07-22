using System;
using System.IO;

namespace Scratch
{
    // This serves as a scratch buffer to test things

    internal class Program
    {
        static void Main(string[] args)
        {
            Test1();
            Test2();
            Test3();
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
        }
    }

    static class Ext
    {
        public static int Abs(this int i) => Math.Abs(i);
        public static double Abs(this double i) => Math.Abs(i);
    }
}