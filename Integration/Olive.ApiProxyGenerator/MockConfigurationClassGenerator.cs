using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.ApiProxy
{
    public class MockConfigurationClassGenerator
    {
        static Type Controller => Context.ControllerType;
        static string ClassName => Controller.Name;
        public static string Generate()
        {
            var r = new StringBuilder();

            r.AppendLine("namespace " + Controller.Namespace);
            r.AppendLine("{");
            r.AppendLine("using System;");
            r.AppendLine("using System.Threading.Tasks;");
            r.AppendLine("using System.Collections.Generic;");
            r.AppendLine("using Olive;");
            r.AppendLine();
            r.Append($"/// <summary>This class will be used to hold metadata for {ClassName} mocking");
            r.AppendLine("</summary>");
            r.AppendLine($"public class {ClassName}MockConfiguration");
            r.AppendLine("{");

            r.AppendLine();

            r.Append($"/// <summary>Configure if mocking is enabled for {ClassName}");
            r.AppendLine("</summary>");
            r.AppendLine("public bool Enabled { get; set; }");
            r.Append($"/// <summary>Defines the mocking behaviour for each method in {ClassName}");
            r.AppendLine("</summary>");
            r.AppendLine($"public {ClassName}Behaviour Expect;");
            //Class defination ends here
            r.AppendLine("}");
            r.AppendLine(GenerateApiBehaviourClass());
            foreach (var method in Context.ActionMethods)
                r.AppendLine(GenerateMethodBehaviourClass(method));
            //Namespace defination ends here
            r.AppendLine("}");
            return new CSharpFormatter(r.ToString()).Format();
        }
        public static string GenerateApiBehaviourClass()
        {
            var r = new StringBuilder();
            r.Append($"/// <summary>set the expected behaviour for {ClassName}");
            r.AppendLine("</summary>");
            r.AppendLine($"public class {ClassName}Behaviour");
            //Class defination starts here
            r.AppendLine("{");
            //define properties for behaviours here
            foreach (var methodInfo in Context.ActionMethods)
            {
                r.AppendLine();
                r.AppendLine($"static {methodInfo.Method.Name}MethodBehaviour {methodInfo.Method.Name}MethodBehaviour;");
            }
            r.AppendLine();
            foreach (var methodInfo in Context.ActionMethods)
            {
                //define method without parameters
                r.AppendLine($"public {methodInfo.Method.Name}MethodBehaviour {methodInfo.Method.Name}()");
                r.AppendLine("{");
                r.AppendLine($"return {methodInfo.Method.Name}MethodBehaviour;");
                r.AppendLine("}");
                if (methodInfo.GetArgs().IsEmpty())
                    continue;

                //define method with parameters
                r.AppendLine($"public {methodInfo.Method.Name}MethodBehaviour {methodInfo.Method.Name}({methodInfo.GetArgs()})");
                r.AppendLine("{");
                r.AppendLine($"return {methodInfo.Method.Name}MethodBehaviour;");
                r.AppendLine("}");
            }
            //Class defination ends here
            r.AppendLine("}");

            return r.ToString();
        }
        public static string GenerateMethodBehaviourClass(MethodGenerator methodInfo)
        {
            var r = new StringBuilder();
            r.Append($"/// <summary>set the expected behaviour for {methodInfo.Method.Name}Method");
            r.AppendLine("</summary>");
            r.AppendLine($"public class {methodInfo.Method.Name}MethodBehaviour");
            //Class defination starts here
            r.AppendLine("{");
            r.AppendLine();
            r.AppendLine($"public void Returns({methodInfo.ReturnType} returnValue)");
            //Method defination starts here
            r.AppendLine("{");
            //Method defination ends here
            r.AppendLine("}");
            //Class defination ends here
            r.AppendLine("}");

            return r.ToString();
        }
    }
}
