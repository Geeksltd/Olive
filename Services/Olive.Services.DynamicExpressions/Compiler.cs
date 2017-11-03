using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace Olive.Framework.Services
{
    internal class Compiler
    {
        List<Assembly> References = new List<Assembly>();

        /// <summary>
        /// Creates a new Compiler instance.
        /// </summary>
        public Compiler()
        {
            Reference<string>();
            Reference<global::System.Linq.IQueryable>();
            Reference<global::System.ComponentModel.WarningException>();
        }

        internal Type CompileClass(string classCode)
        {
            #region Parameters
            var parameters = new CompilerParameters
            {
                GenerateExecutable = false,
                IncludeDebugInformation = true,
                GenerateInMemory = true
                //CompilerOptions = "/optimize"
                /*TreatWarningsAsErrors = false, WarningLevel = 3*/
            };

            // Add references:
            foreach (var dll in References.Distinct().ToArray())
            {
                parameters.ReferencedAssemblies.Add(dll.Location);
            }
            #endregion

            #region Compiler Options
            var options = new Dictionary<string, string>();
            //options.Add("CompilerVersion", "v3.5");
            options.Add("CompilerVersion", "v4.0");
            #endregion

            using (var codeProvider = new CSharpCodeProvider(options))
            {
                var compileResult = codeProvider.CompileAssemblyFromSource(parameters, new[] { classCode });

                EvaluateResult(compileResult);

                var types = compileResult.CompiledAssembly.GetTypes();
                if (types.None())
                    throw new Exception("The dynamic type for the following class has no type, also no error messages were produced by the compiler:\r\n" +
                classCode);

                
                return types.Single(t => t.Name == "Class");
            }
        }

        public void Reference<T>(bool ignoreFailedAssemblies = false)
        {
            Reference(typeof(T), ignoreFailedAssemblies);
        }

        public void Reference(Type type, bool ignoreFailedAssemblies = false)
        {
            Reference(type.Assembly, ignoreFailedAssemblies);
        }

        static ConcurrentDictionary<Assembly, string> _AssemblyFullNames = new ConcurrentDictionary<Assembly, string>();
        static string GetAssemblyFullName(Assembly assembly)
        {
            return _AssemblyFullNames.GetOrAdd(assembly, a => a.GetName().FullName);
        }

        bool IsReferenceAdded(Assembly assembly)
        {
            return References.Any(r => GetAssemblyFullName(r) == GetAssemblyFullName(assembly));
        }

        public void Reference(Assembly assembly, bool ignoreFailedAssemblies = false)
        {
            if (IsReferenceAdded(assembly)) return;

            References.Add(assembly);

            var currentlyLoaded = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var name in assembly.GetReferencedAssemblies())
            {
                try
                {
                    if (References.Any(re => GetAssemblyFullName(re) == name.FullName)) continue;

                    var matched = currentlyLoaded.Where(x => GetAssemblyFullName(x) == name.FullName).ToArray();

                    var toRef = matched.LastOrDefault();

                    //Assembly.ReflectionOnlyLoad(
                    //var r = Assembly.Load(name);
                    if (toRef == null) continue;

                    Reference(toRef);
                }
                catch
                {
                    if (!ignoreFailedAssemblies)
                        throw;
                    // Ignore this assembly.
                }
            }
        }
        
        private void EvaluateResult(CompilerResults result)
        {
            if (result.Errors.Count == 0) return;

            var errors = new List<string>();
            var messages = new List<string>();

            foreach (var error in result.Errors.Cast<CompilerError>().Where(e => !e.IsWarning))
            {
                if (messages.Contains(error.ErrorText)) continue;
                else messages.Add(error.ErrorText);

                errors.AddFormat("{0} ({1}:{2})", error.ErrorText, error.FileName, error.Line);
            }

            if (messages.Any())
            {
                throw new Exception("I cannot compile the dynamic assembly ", new Exception(errors.ToLinesString()));
            }
        }

        internal Type CompileMethods(string methods)
        {
            var r = new StringBuilder();

            r.AppendLine("using System;");
            r.AppendLine("using System.Collections;");
            r.AppendLine("using System.Linq;");
            r.AppendLine("using System.Collections.Generic;");
            r.AppendLine("using System.Text;");
            r.AppendLine("");
            r.AppendLine("");
            r.AppendLine("using App;");
            r.AppendLine();

            r.AppendLine("public static class Evaluator");
            r.AppendLine("{");

            r.AppendLine(methods);
            r.AppendLine("}");

            return CompileClass(r.ToString());
        }
    }
}