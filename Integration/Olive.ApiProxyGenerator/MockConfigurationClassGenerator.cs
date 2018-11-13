using System;
using System.Linq;
using System.Text;

namespace Olive.ApiProxy
{
    public class MockConfigurationClassGenerator
    {
        Type Controller;
        string ClassName => Controller.Name;
        public MockConfigurationClassGenerator(Type controller)
        {
            Controller = controller;
        }

        public string Generate()
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

        private string GenerateApiBehaviourClass(MethodGenerator methodInfo)
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

            var overloads = Context.ActionMethods.Where(x => x.Method.Name == methodInfo.Method.Name).ToList();

            if (overloads.None(x => x.GetArgs().IsEmpty()))
            {

                // All overloads have a value.
                // Define a generic one.
                foreach (var overload in overloads)
                {
                    // define method with parameters
                    r.Append($"/// <summary>Set the mock expectation for {overload.Method.Name} Method");
                    r.AppendLine("</summary>");
                    r.AppendLine($"public {overload.Method.Name}Expectation {overload.Method.Name}({overload.GetArgs()})");
                    r.AppendLine("{");
                    r.AppendLine($"var key=$\"{overload.GetMockKeyExpression()}\";");
                    r.AppendLine($"return {overload.Method.Name}Expectations[key]=new {overload.Method.Name}Expectation();");
                    r.AppendLine("}");
                }

            }
            foreach (var overload in overloads)
            {
                // define result method with parameters
                r.Append($"/// <summary>Get the expected result for {overload.Method.Name}().");
                r.AppendLine("</summary>");
                r.AppendLine($"internal Task{overload.ReturnType.WithWrappers("<", ">")} {overload.Method.Name}Result({overload.GetArgs()})");
                r.AppendLine("{");

                if (methodInfo.HasReturnType())
                {
                    r.AppendLine($"{overload.Method.Name}Expectation expectation;");
                    if (overload.GetArgs().IsEmpty())
                    {
                        r.AppendLine($"if({overload.Method.Name}Expectations.ContainsKey(\"\"))");
                    }
                    else
                    {
                        r.AppendLine($"var key=$\"{overload.GetMockKeyExpression()}\";");
                        r.AppendLine($"if({overload.Method.Name}Expectations.ContainsKey(key))");
                        r.AppendLine($"expectation= {methodInfo.Method.Name}Expectations[key];");
                        r.AppendLine($"else if({overload.Method.Name}Expectations.ContainsKey(\"\"))");
                    }
                    r.AppendLine($"expectation= {methodInfo.Method.Name}Expectations[\"\"];");
                    r.AppendLine("else");
                    r.AppendLine($"throw new Exception(\"Mock expectation is not defined for {overload.Method.Name}().\");");
                    r.AppendLine($"return Task.FromResult(expectation.Result ?? expectation?.FuncResult.Invoke({methodInfo.GetArgsNames()}));");
                }
                else
                {
                    r.AppendLine($"{overload.Method.Name}Expectation expectation;");
                    if (overload.GetArgs().IsEmpty())
                    {
                        r.AppendLine($"if({overload.Method.Name}Expectations.ContainsKey(\"\"))");
                    }
                    else
                    {
                        r.AppendLine($"var key=$\"{overload.GetMockKeyExpression()}\";");
                        r.AppendLine($"if({overload.Method.Name}Expectations.ContainsKey(key))");
                        r.AppendLine($"expectation= {methodInfo.Method.Name}Expectations[key];");
                        r.AppendLine($"else if({overload.Method.Name}Expectations.ContainsKey(\"\"))");
                    }
                    r.AppendLine($"expectation= {methodInfo.Method.Name}Expectations[\"\"];");
                    r.AppendLine("else");
                    r.AppendLine($"throw new Exception(\"Mock expectation is not defined for {overload.Method.Name}().\");");

                    r.AppendLine("if(expectation.Result.HasValue)");
                    r.AppendLine("{");
                    r.AppendLine("if(expectation.Result.Value)");
                    r.AppendLine("return Task.CompletedTask;");
                    r.AppendLine("else");
                    r.AppendLine("throw expectation.InvocationFailedException;");
                    r.AppendLine("}");
                    r.AppendLine("else");
                    r.AppendLine("{");
                    r.AppendLine($"expectation.ActionResult.Invoke({methodInfo.GetArgsNames()});");
                    r.AppendLine("return Task.CompletedTask;");
                    r.AppendLine("}");

                }
                r.AppendLine("}");
                r.AppendLine();
            }
            if (methodInfo.HasReturnType())
                r.AppendLine(GenerateMethodExpectationClass(methodInfo));
            else
                r.AppendLine(GenerateVoidMethodExpectationClass(methodInfo));

            r.AppendLine("}");
            return r.ToString();
        }

        private string GenerateVoidMethodExpectationClass(MethodGenerator methodInfo)
        {
            var r = new StringBuilder();
            r.Append($"/// <summary>set the expected behaviour for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"public class {methodInfo.Method.Name}Expectation");
            r.AppendLine("{");
            r.AppendLine();
            r.AppendLine("internal Nullable<bool> Result;");
            r.AppendLine("internal Exception InvocationFailedException;");
            r.AppendLine($"internal Action<{methodInfo.GetArgsTypes()}> ActionResult;");
            r.AppendLine();
            r.Append("/// <summary>Passes the method execult with no result");
            r.AppendLine("</summary>");
            r.AppendLine("public void Passes()");
            r.AppendLine("{");
            r.AppendLine("Result = true;");
            r.AppendLine("}");
            r.Append("/// <summary>Fails the method execution with an exception");
            r.AppendLine("</summary>");
            r.AppendLine("public void Throws(string message)");
            r.AppendLine("{");
            r.AppendLine("Result = false;");
            r.AppendLine("InvocationFailedException = new Exception(message);");
            r.AppendLine("}");
            r.Append("/// <summary>Fails the method execution with an exception");
            r.AppendLine("</summary>");
            r.AppendLine("public void Throws(Exception ex)");
            r.AppendLine("{");
            r.AppendLine("Result = false;");
            r.AppendLine("InvocationFailedException = ex;");
            r.AppendLine("}");

            r.Append("/// <summary>Executes an expression without returning value");
            r.AppendLine("</summary>");
            r.AppendLine($"public void Does(Action<{methodInfo.GetArgsTypes()}> action)");
            r.AppendLine("{");
            r.AppendLine("Result = null;");
            r.AppendLine("ActionResult = action;");
            r.AppendLine("}");

            r.AppendLine("}");
            return r.ToString();
        }
        private string GenerateMethodExpectationClass(MethodGenerator methodInfo)
        {
            var r = new StringBuilder();
            r.Append($"/// <summary>set the expected behaviour for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"public class {methodInfo.Method.Name}Expectation");
            r.AppendLine("{");
            r.AppendLine();
            r.Append($"/// <summary>Get the configured mock value for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"internal {methodInfo.ReturnType} Result;");
            r.Append($"/// <summary>Get the configured mock Func for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"internal Func<{methodInfo.GetArgsTypes().WithWrappers("", ",")}{methodInfo.ReturnType}> FuncResult;");
            r.AppendLine();
            r.Append($"/// <summary>Set the mock returned value for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"public void Returns({methodInfo.ReturnType} expectedResult)");
            r.AppendLine("{");
            r.AppendLine("Result = expectedResult;");
            r.AppendLine("}");

            r.Append($"/// <summary>Set the mock Func to execute when calling {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"public void Returns(Func<{methodInfo.GetArgsTypes().WithWrappers("", ",")}{methodInfo.ReturnType}> expectedFuncResult)");
            r.AppendLine("{");
            r.AppendLine("FuncResult = expectedFuncResult;");
            r.AppendLine("}");
            r.AppendLine("}");
            return r.ToString();
        }
        static string GetKey(string args) => args.Replace(",", "_");
        static bool HasMethodsWithParams(MethodGenerator methodInfo) => Context.ActionMethods.Any(x => x.Method.Name == methodInfo.Method.Name && !x.GetArgs().IsEmpty());

    }
}