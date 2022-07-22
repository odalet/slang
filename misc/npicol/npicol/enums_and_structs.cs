using System;
using System.Collections.Generic;

namespace NPicol
{
    internal enum Status
    {
        OK,
        Error,
        Return,
        Break,
        Continue
    }

    internal enum TokenType
    {
        Esc,
        Str,
        Cmd,
        Var,
        Sep,
        Eol,
        Eof
    };

    internal sealed class Variable
    {
        public Variable(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name;
        public string Value;
        public Variable? Next;
    }

    internal delegate Status CommandFunction(InterpreterData i, string[] args, object? privateData);

    internal sealed class Command
    {
        public Command(string name, CommandFunction func, object? privateData = null)
        {
            Name = name;
            Func = func;
            PrivateData = privateData;
        }

        public string Name;
        public CommandFunction Func;
        public object? PrivateData;
        ////public Command? Next;
    }

    internal sealed class CallFrame
    {
        public Dictionary<string, Variable> Variables { get; } = new();
        public CallFrame? Parent; // null at top level
    }
}
