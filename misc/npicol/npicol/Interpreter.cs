using System;
using System.Collections.Generic;

namespace NPicol
{
    internal sealed class Interpreter
    {
        private readonly InterpreterData data = new();

        public Interpreter() => RegisterCoreCommands();

        public string Result => data.Result;

        public Status Evaluate(string source, bool dumpDebugInformation = false)
        {
            // \0: hack so that we don't have to test for the end of the string
            // in each sub-parser...
            var p = new Parser(source + '\0');
            if (dumpDebugInformation) p.Dump();
            data.SetResult("");

            var arguments = new List<string>();

            while (true)
            {
                var previousType = p.Type;

                _ = p.ConsumeNextToken();
                if (dumpDebugInformation) p.Dump();
                if (p.Type == TokenType.Eof) break;

                var tok = p.Token;
                if (p.Type == TokenType.Var)
                {
                    var variable = data.GetVariable(tok);
                    if (variable == null)
                    {
                        data.SetResult($"Undefined variable: '{tok}'");
                        return Status.Error;
                    }

                    tok = variable.Value; // Replace!
                }
                else if (p.Type == TokenType.Cmd)
                {
                    var status = Evaluate(tok);
                    if (status != Status.OK) return status;
                    tok = data.Result;
                }
                else if (p.Type == TokenType.Esc)
                {
                    // TODO
                }
                else if (p.Type == TokenType.Sep)
                {
                    previousType = p.Type;
                    continue;
                }
                else if (p.Type == TokenType.Eol)
                {
                    previousType = p.Type;
                    if (arguments.Count > 0)
                    {
                        var command = data.GetCommand(arguments[0]);
                        if (command == null)
                        {
                            data.SetResult($"Undefined command: '{arguments[0]}'");
                            return Status.Error;
                        }

                        var status = command.Func(data, arguments.ToArray(), command.PrivateData);
                        if (status != Status.OK) return status;
                    }

                    arguments.Clear();
                    continue;
                }

                // We have a new token, append to the previous or as new arg
                if (previousType is TokenType.Sep or TokenType.Eol)
                    arguments.Add(tok);
                else // Interpolation
                    arguments[^1] = arguments[^1] + tok;

                previousType = p.Type;
            }

            return Status.OK;
        }

        private void RegisterCoreCommands()
        {
            foreach (var op in new[] { "+", "-", "*", "/", ">", ">=", "<", "<=", "==", "!=" })
                RegisterCommand(op, RuntimeCommands.MathCommand, null);

            RegisterCommand("set", RuntimeCommands.SetCommand, null);
            RegisterCommand("puts", RuntimeCommands.PutsCommand, null);
        }

        private void RegisterCommand(string name, CommandFunction function, object? privateData)
        {
            if (data.Commands.ContainsKey(name))
                throw new ArgumentException($"Command '{name}' was already registered", nameof(name));
            data.Commands.Add(name, new Command(name, function, privateData));
        }
    }
}
