using System.Collections.Generic;

namespace NPicol
{
    internal sealed class InterpreterData
    {
        public InterpreterData()
        {
            CallFrame = new();
            Commands = new();
            Result = "";
        }

        public int Level; // Level of nesting
        public CallFrame CallFrame { get; }
        public Dictionary<string, Command> Commands { get; }
        public string Result { get; private set; }

        public void SetResult(string result) => Result = result;

        public Status SetVariable(string name, string value)
        {
            var variable = GetVariable(name);
            if (variable == null)
            {
                variable = new Variable(name, value);
                CallFrame.Variables.Add(name, variable);
            }
            else variable.Value = value;

            return Status.OK;
        }

        public Command? GetCommand(string name) => 
            Commands.ContainsKey(name) ? Commands[name] : null;

        public Variable? GetVariable(string name) =>
            CallFrame.Variables.ContainsKey(name) ? CallFrame.Variables[name] : null;
    }

    internal static class InterpreterDataExtensions
    {

        public static void SetResult(this InterpreterData i, long result) => i.SetResult(result.ToString());
    }
}
