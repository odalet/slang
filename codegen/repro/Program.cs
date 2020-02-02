using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace repro
{
    // Build as a.netcoreapp3.0 project
    internal sealed class Program
    {
        [STAThread]
        private static void Main()
        {
            var outputFile = @"c:\temp\app.dll";
            new Program().Run(outputFile);
        }

        private void Run(string filename)
        {
            const string assemblyAndMainModuleName = "app";

            var assemblyNameDefinition = new AssemblyNameDefinition(assemblyAndMainModuleName, new Version(1, 0, 0, 0));
            using (var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyNameDefinition, assemblyAndMainModuleName, ModuleKind.Console))
            {
                var mainModule = assemblyDefinition.MainModule;
                mainModule.RuntimeVersion = "v4.0.30319";

                var type = new TypeDefinition("app", "Program", TypeAttributes.NotPublic | TypeAttributes.Sealed, mainModule.TypeSystem.Object)
                {
                    IsBeforeFieldInit = true
                };

                var main = new MethodDefinition("Main", MethodAttributes.Private | MethodAttributes.Static, mainModule.TypeSystem.Void);

                var writeLine = mainModule.ImportReference(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));

                var il = main.Body.GetILProcessor();
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldstr, "Hello, World!");
                il.Emit(OpCodes.Call, writeLine);
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ret);

                type.Methods.Add(main);
                mainModule.Types.Add(type);

                mainModule.EntryPoint = main;

                assemblyDefinition.EntryPoint = main;
                assemblyDefinition.Write(filename);
            }
        }
    }
}
