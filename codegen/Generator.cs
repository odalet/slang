using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ILGen
{
    internal enum TargetFramework
    {
        Core,
        Fx
    }

    internal sealed class Generator
    {
        public Generator(TargetFramework target, string outputFileName)
        {
            TargetFramework = target;
            OutputFileName = outputFileName;
        }

        private TargetFramework TargetFramework { get; }
        private string OutputFileName { get; }

        public void Run()
        {
            const string assemblyAndMainModuleName = "app";

            var assemblyNameDefinition = new AssemblyNameDefinition(assemblyAndMainModuleName, new Version(1, 0, 0, 0));
            using (var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyNameDefinition, assemblyAndMainModuleName, ModuleKind.Console))
            {
                var mainMethod = Generate1(assemblyDefinition);

                // set the entry point and save the module
                assemblyDefinition.EntryPoint = mainMethod;
                assemblyDefinition.Write(OutputFileName);
            }
        }

        private MethodDefinition Generate1(AssemblyDefinition assemblyDefinition)
        {
            var mainModule = assemblyDefinition.MainModule;
            mainModule.RuntimeVersion = "v4.0.30319";

            var type = new TypeDefinition("slang", "Program", TypeAttributes.NotPublic | TypeAttributes.Sealed, mainModule.TypeSystem.Object)
            {
                IsBeforeFieldInit = true
            };

            var main = new MethodDefinition("Main", MethodAttributes.Private | MethodAttributes.Static, mainModule.TypeSystem.Void);

            var writeLine = mainModule.Import(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));

            var il = main.Body.GetILProcessor();
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldstr, "Hello, World!");
            il.Emit(OpCodes.Call, writeLine);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);

            type.Methods.Add(main);
            mainModule.Types.Add(type);

            if (TargetFramework == TargetFramework.Core)
            {
                var targetFrameworkAttribute = new CustomAttribute(mainModule.ImportReference(
                    typeof(System.Runtime.Versioning.TargetFrameworkAttribute).GetConstructor(new[] { typeof(string) })));
                targetFrameworkAttribute.ConstructorArguments.Add(
                    new CustomAttributeArgument(mainModule.TypeSystem.String, ".NETCoreApp,Version=v3.1"));
            }

            mainModule.EntryPoint = main;

            return main;
        }

        private MethodDefinition Generate2(AssemblyDefinition assemblyDefinition)
        {            
            var mainModule = assemblyDefinition.MainModule;

            // create the program type and add it to the module
            var programType = new TypeDefinition("HelloWorld", "Program",
                TypeAttributes.Class | TypeAttributes.Public, mainModule.TypeSystem.Object);

            mainModule.Types.Add(programType);

            // add an empty constructor
            var ctor = new MethodDefinition(".ctor", 
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, 
                mainModule.TypeSystem.Void);

            // create the constructor's method body
            var il = ctor.Body.GetILProcessor();

            il.Append(il.Create(OpCodes.Ldarg_0));

            // call the base constructor
            il.Append(il.Create(OpCodes.Call, 
                mainModule.Import(typeof(object).GetConstructor(Array.Empty<Type>()))));

            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ret));

            programType.Methods.Add(ctor);

            // define the 'Main' method and add it to 'Program'
            var mainMethod = new MethodDefinition("Main",
                MethodAttributes.Public | MethodAttributes.Static, mainModule.TypeSystem.Void);

            programType.Methods.Add(mainMethod);

            // add the 'args' parameter
            var argsParameter = new ParameterDefinition("args", ParameterAttributes.None, 
                mainModule.Import(typeof(string[])));

            mainMethod.Parameters.Add(argsParameter);

            // create the method body
            il = mainMethod.Body.GetILProcessor();

            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ldstr, "Hello World"));

            var writeLineMethod = il.Create(OpCodes.Call,
                mainModule.Import(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) })));

            // call the method
            il.Append(writeLineMethod);

            il.Append(il.Create(OpCodes.Nop));
            il.Append(il.Create(OpCodes.Ret));

            return mainMethod;
        }
    }
}
