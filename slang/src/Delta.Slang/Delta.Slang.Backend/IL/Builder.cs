using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyModel;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Delta.Slang.Backend.IL
{
    public sealed class AssemblyResolutionException : Exception
    {
        public AssemblyResolutionException(AssemblyNameReference reference) : base(string.Format("Failed to resolve assembly: '{0}'", reference)) => AssemblyReference = reference;
        public AssemblyNameReference AssemblyReference { get; }
    }

    // From https://github.com/jbevain/cecil/issues/306
    internal class DotNetCoreAssemblyResolver : IAssemblyResolver
    {
        readonly Dictionary<string, Lazy<AssemblyDefinition>> libraries;

        public DotNetCoreAssemblyResolver()
        {
            libraries = new Dictionary<string, Lazy<AssemblyDefinition>>();

            var compileLibraries = DependencyContext.Default.CompileLibraries;
            foreach (var library in compileLibraries)
            {
                var path = library.ResolveReferencePaths().FirstOrDefault();
                if (string.IsNullOrEmpty(path))
                    continue;

                libraries.Add(library.Name, new Lazy<AssemblyDefinition>(() => AssemblyDefinition.ReadAssembly(path, new ReaderParameters() { AssemblyResolver = this })));
            }
        }

        public virtual AssemblyDefinition Resolve(string fullName) => Resolve(fullName, new ReaderParameters());

        public virtual AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            if (fullName == null) throw new ArgumentNullException("fullName");
            return Resolve(AssemblyNameReference.Parse(fullName), parameters);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name) => Resolve(name, new ReaderParameters());

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (libraries.TryGetValue(name.Name, out var asm))
                return asm.Value;

            throw new AssemblyResolutionException(name);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            foreach (var lazy in libraries.Values)
            {
                if (!lazy.IsValueCreated)
                    continue;

                lazy.Value.Dispose();
            }
        }
    }

    public sealed class Builder
    {
        public Builder(string filename)
        {
            FileName = filename;
            //AssemblyResolver = new DotNetCoreAssemblyResolver();
            AssemblyResolver = new DefaultAssemblyResolver();
        }

        private string FileName { get; }
        public void Build()
        {
            Console.WriteLine("Building...");

            GenerateAssembly();

            Console.WriteLine("Done");
        }

        private IAssemblyResolver AssemblyResolver { get; }

        private void GenerateAssembly()
        {
            var assy = AssemblyDefinition.ReadAssembly(@"D:\WORK\REPOSITORIES\_odalet\lang-master\binaries\netcoreapp3.1-debug\ConsoleApp5.dll");
            var foo = 42;

            var assemblyName = new AssemblyNameDefinition("Program", new Version(1, 0, 0, 0));
            var parameters = new ModuleParameters { Kind = ModuleKind.Console };
            using (var assembly = AssemblyDefinition.CreateAssembly(assemblyName, "Program", parameters))
            {
                var module = assembly.MainModule;
                module.RuntimeVersion = "v4.0.30319";
                var typeSystem = module.TypeSystem;



                var writeLineMethodInfo = typeof(Console).GetMethod("WriteLine", new[] { typeof(string) });

                var systemRuntimeReference = new AssemblyNameReference("System.Runtime", new Version(4, 2, 2, 0))
                {
                    PublicKeyToken = new byte[] { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a }
                };
                var systemRuntime = AssemblyResolver.Resolve(systemRuntimeReference);

                var systemPrivateCoreLibReference = systemRuntime.MainModule.AssemblyReferences.Single(r => r.Name == "System.Private.CoreLib");
                var systemPrivateCoreLib = AssemblyResolver.Resolve(systemPrivateCoreLibReference);

                //var systemObject = systemRuntime.MainModule.ExportedTypes.Single(t => t.Name == "Object" && t.Namespace == "System").Resolve();
                var systemObject = systemRuntime.MainModule.TypeSystem.Object;
                systemObject.Scope = systemRuntimeReference;

                //new TypeReference("System", "Object", module, systemRuntimeReference);
                var systemVoid = systemRuntime.MainModule.TypeSystem.Void;
                //new TypeReference("System", "Void", module, systemRuntimeReference);
                //systemRuntime.MainModule.ExportedTypes.Single(t => t.Name == "Void" && t.Namespace == "System").Resolve();

                var coreTypeSystem = systemRuntime.MainModule.TypeSystem;


                //var consoleTypeReference = new TypeReference("System", "Console", systemRuntime.MainModule, systemRuntime.MainModule);

                //var typeSystem = coreTypeSystem;

                //_ = module.ImportReference(consoleTypeReference);
                //var writeLine = module.ImportReference(writeLineMethodInfo);

                var _obj = module.ImportReference(systemObject);
                var _void = module.ImportReference(systemVoid);

                var type = new TypeDefinition("slang", "Program", TypeAttributes.NotPublic | TypeAttributes.Sealed, systemObject)
                {
                    IsBeforeFieldInit = true
                };

                var main = new MethodDefinition("Main", MethodAttributes.Private | MethodAttributes.Static, _void);

                var il = main.Body.GetILProcessor();
                il.Emit(OpCodes.Nop);
                //il.Emit(OpCodes.Ldstr, "Hello, World!");
                //il.Emit(OpCodes.Call, writeLine);
                //il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ret);

                type.Methods.Add(main);
                module.Types.Add(type);

                var targetFrameworkAttribute = new CustomAttribute(module.ImportReference(
                    typeof(System.Runtime.Versioning.TargetFrameworkAttribute).GetConstructor(new[] { typeof(string) })));
                targetFrameworkAttribute.ConstructorArguments.Add(new CustomAttributeArgument(typeSystem.String, ".NETCoreApp,Version=v3.1"));

                module.EntryPoint = main;

                ////// Patch the module's references
                ////var found = module.AssemblyReferences.Where(r => r.Name == "System.Private.CoreLib").ToArray();
                ////foreach (var r in found)
                ////    module.AssemblyReferences.Remove(r);

                module.AssemblyReferences.Add(systemRuntime.Name);

                assembly.CustomAttributes.Add(targetFrameworkAttribute);
                assembly.Write(FileName);
            }
        }

        ////private static MethodReference GetWriteLine(TypeReference type)
        ////{
        ////    var typeDefinition = type.Resolve();

        ////    foreach (var md in typeDefinition.Methods.Where(m => m.Name == "WriteLine"))
        ////    {
        ////        if (md.Parameters.Count == 1 && md.Parameters[0].ParameterType == type.Module.TypeSystem.String)
        ////            return md;
        ////    }

        ////    return null;
        ////}
    }
}
