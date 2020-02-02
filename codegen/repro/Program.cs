using System;
using System.Linq;
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

#if NETCOREAPP
            var corlibReference = new AssemblyNameReference("System.Runtime", new Version(4, 2, 1, 0))
            {
                PublicKeyToken = new byte[] { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a }
            };
#else
            var corlibReference = new AssemblyNameReference("mscorlib", new Version(4, 0, 0, 0))
            {
                PublicKeyToken = new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 }
            };
#endif
            //var resolver = new DefaultAssemblyResolver();
            //var corlib = resolver.Resolve(corlibReference);
            //var privlibReference = corlib.MainModule.AssemblyReferences.Single(a => a.Name == "System.Private.CoreLib");
            //var privlib = resolver.Resolve(privlibReference);

            //var stringType = privlib.MainModule.Types.Single(t => t.Namespace == "System" && t.Name == "String");
            //var consoleType = privlib.MainModule.Types.Single(t => t.Namespace == "System" && t.Name == "Console");
            //var writeLineMethod = consoleType.Methods.Single(m => m.Name == "WriteLine" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType == stringType);

            var assemblyNameDefinition = new AssemblyNameDefinition(assemblyAndMainModuleName, new Version(1, 0, 0, 0));
            using (var assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyNameDefinition, assemblyAndMainModuleName, ModuleKind.Console))
            {
                var mainModule = assemblyDefinition.MainModule;
                mainModule.AssemblyReferences.Add(corlibReference);
                mainModule.RuntimeVersion = "v4.0.30319";

                var type = new TypeDefinition("app", "Program", TypeAttributes.NotPublic | TypeAttributes.Sealed, mainModule.TypeSystem.Object)
                {
                    IsBeforeFieldInit = true
                };

                var main = new MethodDefinition("Main", MethodAttributes.Private | MethodAttributes.Static, mainModule.TypeSystem.Void);

                //var writeLine = mainModule.ImportReference(writeLineMethod);
                var writeLine = mainModule.ImportReference(typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));

                var il = main.Body.GetILProcessor();
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldstr, "Hello, World!");
                il.Emit(OpCodes.Call, writeLine);
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ret);

                type.Methods.Add(main);
                mainModule.Types.Add(type);

#if NETCOREAPP
                var targetFrameworkAttribute = new CustomAttribute(mainModule.ImportReference(
                    typeof(System.Runtime.Versioning.TargetFrameworkAttribute).GetConstructor(new[] { typeof(string) })));
                targetFrameworkAttribute.ConstructorArguments.Add(
                    new CustomAttributeArgument(mainModule.TypeSystem.String, ".NETCoreApp,Version=v3.0"));
                targetFrameworkAttribute.Properties.Add(new CustomAttributeNamedArgument("FrameworkDisplayName",
                    new CustomAttributeArgument(mainModule.TypeSystem.String, "")));
#else
                var targetFrameworkAttribute = new CustomAttribute(mainModule.ImportReference(
                    typeof(System.Runtime.Versioning.TargetFrameworkAttribute).GetConstructor(new[] { typeof(string) })));
                targetFrameworkAttribute.ConstructorArguments.Add(
                    new CustomAttributeArgument(mainModule.TypeSystem.String, ".NETFramework,Version=v4.7.2"));
                targetFrameworkAttribute.Properties.Add(new CustomAttributeNamedArgument("FrameworkDisplayName",
                    new CustomAttributeArgument(mainModule.TypeSystem.String, ".NET Framework 4.7.2")));
#endif
                mainModule.EntryPoint = main;
                assemblyDefinition.EntryPoint = main;

                assemblyDefinition.CustomAttributes.Add(targetFrameworkAttribute);
                assemblyDefinition.Write(filename);
            }
        }
    }
}
