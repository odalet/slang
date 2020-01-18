namespace TestApp
{
    class Foo2
    {
        double sum(double i, int j) => i + j;
        double sum(int i, double j) => i + j;
        //double sum(double i, double j) => i + j;
        //double sum(int i, int j) => i + j;

        void foo()
        {
            //var result = sum(1, 2);
        }
    }
}
