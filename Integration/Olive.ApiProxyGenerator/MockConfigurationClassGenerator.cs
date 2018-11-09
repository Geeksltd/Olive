using System;
using System.Linq;
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

            r.AppendLine($"namespace {Controller.Namespace}.Mock");
            r.AppendLine("{");
            r.AppendLine("using System;");
            r.AppendLine("using System.Threading.Tasks;");
            r.AppendLine("using System.Collections.Generic;");
            r.AppendLine("using Olive;");
            r.AppendLine();

            r.Append($"/// <summary>Holds the metadata for {ClassName} mocking.");
            r.AppendLine("</summary>");
            r.AppendLine($"public class {ClassName}MockConfiguration");
            r.AppendLine("{");
            r.AppendLine();

            r.Append($"/// <summary>Configure if mocking is enabled for {ClassName}");
            r.AppendLine("</summary>");
            r.AppendLine("internal bool Enabled { get; set; }");
            r.AppendLine();

            r.Append($"/// <summary>Defines the mocking behaviour for each method in {ClassName}");
            r.AppendLine("</summary>");
            r.AppendLine($"public {ClassName}Behaviour Expect;");
            r.AppendLine();

            r.Append($"/// <summary>Initialize new instance of {ClassName}MockConfiguration");
            r.AppendLine("</summary>");
            r.AppendLine($"public {ClassName}MockConfiguration()");
            r.AppendLine("{");
            r.AppendLine($"Expect = new {ClassName}Behaviour();");
            r.AppendLine("}");
            r.AppendLine("}");

            foreach (var method in Context.ActionMethods.Distinct(x => x.Method.Name))
                r.AppendLine(GenerateApiBehaviourClass(method));

            r.AppendLine("}");
            return new CSharpFormatter(r.ToString()).Format();
        }

        public static string GenerateApiBehaviourClass(MethodGenerator methodInfo)
        {
            var r = new StringBuilder();
            r.Append($"/// <summary>Expected behaviour of the mocked {ClassName}.{methodInfo.Method.Name}() method.");
            r.AppendLine("</summary>");
            r.AppendLine($"public partial class {ClassName}Behaviour");
            r.AppendLine("{");

            r.AppendLine($"Dictionary<string,{methodInfo.Method.Name}Expectation> {methodInfo.Method.Name}Expectations");
            r.AppendLine($"= new Dictionary<string,{methodInfo.Method.Name}Expectation>();");
            r.AppendLine();

            // define method without parameters
            r.Append($"/// <summary>Sets the default expected mock result for when {methodInfo.Method.Name}() is called, irrespective of input arguments.");
            r.AppendLine("</summary>");
            r.AppendLine($"public {methodInfo.Method.Name}Expectation {methodInfo.Method.Name}()");
            r.Append(" => ");
            r.AppendLine($"{methodInfo.Method.Name}Expectations[\"\"] = new {methodInfo.Method.Name}Expectation();");
            r.AppendLine();

            var overloads = Context.ActionMethods.Where(x => x.Method.Name == methodInfo.Method.Name);

            if (overloads.None(x => x.GetArgs().IsEmpty()))
            {
                // All overloads have a value.
                // Define a generic one.
                foreach (var overload in overloads)
                {
                    // define method with parameters
                    r.Append($"/// <summary>Set the expected mock result for when {overload.Method.Name}() is called with a particular argument.");
                    r.AppendLine("</summary>");
                    r.AppendLine($"public {overload.Method.Name}Expectation {overload.Method.Name}({overload.GetArgs()})");
                    r.AppendLine("{");
                    r.AppendLine($"var key = $\"{overload.GetMockKeyExpression()}\";");
                    r.AppendLine($"return {overload.Method.Name}Expectations[key] = new {overload.Method.Name}Expectation();");
                    r.AppendLine("}");
                    r.AppendLine();
                }
            }

            foreach (var overload in overloads)
            {
                // define result method with parameters
                r.Append($"/// <summary>Get the expected result for {overload.Method.Name}().");
                r.AppendLine("</summary>");
                r.AppendLine($"internal {overload.ReturnType} {overload.Method.Name}Result({overload.GetArgs()})");
                r.AppendLine("{");
                r.AppendLine($"var key = $\"{overload.GetMockKeyExpression()}\";");
                r.AppendLine();
                r.AppendLine($"if({overload.Method.Name}Expectations.TryGetValue(key, out var result))");
                r.AppendLine("return result.Result;");
                r.AppendLine();
                r.AppendLine($"if({overload.Method.Name}Expectations.TryGetValue(\"\", out result))");
                r.AppendLine("return result.Result;");
                r.AppendLine();
                r.AppendLine($"throw new Exception(\"Mock expectation is not defined for {overload.Method.Name}().\");");
                r.AppendLine("}");
                r.AppendLine();
            }

            r.AppendLine(GenerateMethodBehaviourClass(methodInfo));

            r.AppendLine("}");

            return r.ToString();
        }

        public static string GenerateMethodBehaviourClass(MethodGenerator methodInfo)
        {
            var r = new StringBuilder();
            r.Append($"/// <summary>set the expected behaviour for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"public class {methodInfo.Method.Name}Expectation");
            // Class defination starts here
            r.AppendLine("{");
            r.AppendLine();

            r.AppendLine($"internal {methodInfo.ReturnType} Result;");
            r.AppendLine();
            r.Append("/// <summary>Set the mock value that this method will return when invoked.");
            r.AppendLine("</summary>");
            r.AppendLine($"public void Returns({methodInfo.ReturnType} expectedResult)");
            // Method defination starts here
            r.AppendLine("{");
            r.AppendLine("Result = expectedResult;");
            // Method defination ends here
            r.AppendLine("}");
            // Class defination ends here
            r.AppendLine("}");

            return r.ToString();
        }

        static string GetKey(string args) => args.Replace(",", "_");
    }
}