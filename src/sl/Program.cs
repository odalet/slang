using System;
using System.Diagnostics;
using System.IO;
using Slang.CodeAnalysis.Text;
using System.Text;

namespace Slang.Cli
{
    internal sealed class Program
    {
        private enum ExitCode
        {
            // See https://www.freebsd.org/cgi/man.cgi?query=sysexits&apropos=0&sektion=0&manpath=FreeBSD+4.3-RELEASE&format=html
            OK = 0,
            InvalidUsage = 64,
            DataError = 65,
            NoInput = 66
        }

        public Program()
        {
            Reader = Console.In;
            Writer = Console.Out;
            ErrWriter = Console.Error;
        }

        private static int Main(string[] args) => (int)new Program().Run(args);

        private TextReader Reader { get; }
        private TextWriter Writer { get; }
        private TextWriter ErrWriter { get; }

        private ExitCode Run(string[] args)
        {
            var exeName = Process.GetCurrentProcess().ProcessName;
            if (args.Length > 1)
            {
                ErrWriter.WriteLine($"Usage: {exeName} [script]");
                return ExitCode.InvalidUsage;
            }

            if (args.Length == 1)
            {
                var filename = args[0];
                if (!Path.IsPathRooted(filename))
                    filename = Path.Combine(Environment.CurrentDirectory, filename);

                if (!File.Exists(filename))
                {
                    ErrWriter.WriteLine($"File not found: {filename}");
                    return ExitCode.NoInput;
                }

                return RunFile(filename);
            }

            return RunCli();
        }

        private ExitCode RunFile(string filename)
        {
            Writer.WriteLine($"> Loading {filename}");
            var watch = new Stopwatch();

            using var reader = new StreamReader(File.OpenRead(filename));
            watch.Reset();
            watch.Start();
            ProcessInput(reader);
            watch.Stop();

            Writer.WriteLine($"> Processing time: {TimeSpan.FromTicks(watch.ElapsedTicks)}");
            return ExitCode.OK;
        }

        private ExitCode RunCli()
        {
            Writer.WriteLine("Enter ;; to exit");

            const string prompt = "sl> ";

            while (true)
            {
                Writer.Write(prompt);
                var line = Reader.ReadLine() ?? "";
                if (line.Trim() == ";;")
                    break;

                ProcessInput(line);
            }

            return ExitCode.OK;
        }

        private void ProcessInput(TextReader reader) => ProcessInput(SourceText.From(reader, Encoding.UTF8));
        private void ProcessInput(string source) => ProcessInput(SourceText.From(source, Encoding.UTF8));
        private void ProcessInput(SourceText sourceText)
        {
            Writer.WriteLine($"Processing '{sourceText}'");
        }
    }
}
