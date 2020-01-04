using System;

namespace TestApp
{
    internal static class Foo1
    {
        public static void Test()
        {
            //Console.WriteLine($"Pre : {Pre(0)}");
            //Console.WriteLine($"Post: {Post(0)}");
        }

        //public static void PreAssignExplicit()
        //{
        //    var i = 0;
        //    var j = i = i + 1;
        //}

        //public static void PostAssignExplicit()
        //{
        //    var i = 0;
        //    var j = i;
        //    i = i + 1;
        //}

        //public static void PostSimple()
        //{
        //    var i = 0;
        //    i++;
        //}

        //public static void PreSimple()
        //{
        //    var i = 0;
        //    ++i;
        //}

        //public static void PostAssignSimple()
        //{
        //    var i = 0;
        //    var j = i++;
        //}

        //public static void PostAssignParen()
        //{
        //    var i = 0;
        //    var j = (i++);
        //}

        //public static void PreAssignSimple()
        //{
        //    var i = 0;
        //    var j = ++i;
        //}

        //public static int Pre(int i)
        //{
        //    i = ++i + ++i;
        //    return i;
        //}

        //public static int Post(int i)
        //{
        //    i = i++ + i++;
        //    return i;
        //}

        public static void PostComplex()
        {
            var i = 0;
            var j = i++ + i++;
        }

        public static void PreComplex()
        {
            var i = 0;
            var j = ++i + ++i;
        }

        public static void PreComplexExplicit()
        {
            var i = 0;
            var j = (i = i + 1) + (i = i + 1);
        }

        public static void Decrement()
        {
            var i = 0;
            var j = --i;
        }
    }
}
