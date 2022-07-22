using System;

namespace NPicol
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var program = @"
# set x 42
# puts x
# puts $x
# NB: the : below should not be required; but if not present, the command is not executed...
puts [ + 1 2 ; ]
set expr { + 1 2 ; }
puts expr
puts $expr
# puts [ expr ; ]
puts [ $expr ; ]
";
            var interpreter = new Interpreter();
            var status = interpreter.Evaluate(program);
            if (status == Status.OK)
                Console.WriteLine("Evaluation was successful");
            else
            {
                Console.WriteLine($"Evaluation Status: {status}");
                Console.WriteLine($"\tLast Result: {interpreter.Result}");
            }
        }
    }
}
