using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteDB;
using Microsoft.CSharp;

namespace Shared.Core
{
    public static class ScriptEngine
    {
        public static List<IScript> Scripts = new List<IScript>();
        public static List<IScript> ScriptsWithContents => Scripts.Where(script => script.GetType().GetInterfaces().Any(interfaces => interfaces == typeof(IContent))).ToList();

        public static void LoadAllScripts(Client client)
        {
            LoadAssembly(Assembly.GetEntryAssembly());
            Scripts.ForEach(client.LoadEvents);
        }

        private static void CompileAssembly(params string[] codes)
        {
            var provider = new CSharpCodeProvider();
            var compilerParams = new CompilerParameters { GenerateInMemory = true, TreatWarningsAsErrors = false, GenerateExecutable = false, CompilerOptions = "/optimize" };
            compilerParams.ReferencedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic).Select(assembly => assembly.Location).ToArray());

            var compile = provider.CompileAssemblyFromSource(compilerParams, codes);
            if (compile.Errors.HasErrors) { throw new Exception(compile.Errors.Cast<CompilerError>().Aggregate("Compile Error: ", (current, ce) => $"{current}\r\n{ce.ToString()}")); }

            LoadAssembly(compile.CompiledAssembly);
        }

        private static void LoadAssembly(Assembly assembly)
        {
            var scriptTypeList = assembly.GetTypes().Where(mod => mod.GetInterfaces().Any(type => type == typeof(IScript)));
            foreach (var scriptType in scriptTypeList)
            {
                var script = (IScript)Activator.CreateInstance(scriptType);
                var haveIDatabaseType = scriptType.GetInterfaces().FirstOrDefault(type => type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDatabase<>));

                if (haveIDatabaseType != null)
                {
                    var genericType = haveIDatabaseType.GetGenericArguments()[0];
                    var genericValue =
                        typeof(LiteDatabase).GetMember("GetCollection")
                            .Cast<MethodInfo>()
                            .First(meth => meth.IsGenericMethod)
                            .MakeGenericMethod(genericType)
                            .Invoke(Data.LiteDatabase, new object[] { $"-{script.Name.Replace(" ", "")}_{genericType.Name}" });
                    typeof(IDatabase<>).MakeGenericType(genericType)
                        .GetProperty("Database")
                        .SetValue(script, genericValue, null);
                }

                script.Loaded();
                Scripts.Add(script);
            }
        }
    }
}