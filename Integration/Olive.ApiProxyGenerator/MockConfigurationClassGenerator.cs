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
            //if (HasMethodsWithParams(methodInfo))
            //{
            //    r.Append($"/// <summary>This will holds the expectation results for {methodInfo.Method.Name} Method");
            //    r.AppendLine("</summary>");
            //    r.AppendLine($"Dictionary<string,{methodInfo.Method.Name}WithParamExpectation> {methodInfo.Method.Name}WithParamExpectations = new Dictionary<string,{methodInfo.Method.Name}WithParamExpectation>();");
            //}
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
                r.AppendLine($"var key=$\"{overload.GetMockKeyExpression()}\";");
                if (methodInfo.HasReturnType())
                {
                    r.AppendLine($"{overload.Method.Name}Expectation expectation;");
                    r.AppendLine($"if({overload.Method.Name}Expectations.ContainsKey(key))");
                    r.AppendLine($"expectation= {methodInfo.Method.Name}Expectations[key];");
                    r.AppendLine($"else if({overload.Method.Name}Expectations.ContainsKey(\"\"))");
                    r.AppendLine($"expectation= {methodInfo.Method.Name}Expectations[\"\"];");
                    r.AppendLine("else");
                    r.AppendLine($"throw new Exception(\"Mock expectation is not defined for {overload.Method.Name}().\");");
                    r.AppendLine($"return Task.FromResult(expectation.Result ?? expectation?.FuncResult.Invoke({methodInfo.GetArgsNames()}));");
                }
                else
                {
                    r.AppendLine($"if({overload.Method.Name}Expectations.ContainsKey(key))");
                    r.AppendLine("{");
                    r.AppendLine($"var expectation = {overload.Method.Name}Expectations[key];");
                    r.AppendLine("if(expectation.Result)");
                    r.AppendLine("return Task.CompletedTask;");
                    r.AppendLine("else");
                    r.AppendLine("throw new Exception(expectation.ErrorMessage);");
                    r.AppendLine("}");
                    r.AppendLine("else");
                    r.AppendLine("{");
                    r.AppendLine($"var expectation = {overload.Method.Name}Expectations[\"\"];");
                    r.AppendLine("if(expectation.Result)");
                    r.AppendLine("return Task.CompletedTask;");
                    r.AppendLine("else");
                    r.AppendLine("throw new Exception(expectation.ErrorMessage);");
                    r.AppendLine("}");
                }
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
            if (methodInfo.HasReturnType())
            {
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
                //Method defination starts here
                r.AppendLine("{");
                r.AppendLine("Result = expectedResult;");
                //Method defination ends here
                r.AppendLine("}");

                r.Append($"/// <summary>Set the mock Func to execute when calling {methodInfo.Method.Name} Method");
                r.AppendLine("</summary>");
                r.AppendLine($"public void Returns(Func<{methodInfo.GetArgsTypes().WithWrappers("", ",")}{methodInfo.ReturnType}> expectedFuncResult)");
                //Method defination starts here
                r.AppendLine("{");
                r.AppendLine("FuncResult = expectedFuncResult;");
                //Method defination ends here
                r.AppendLine("}");
            }
            else
            {
                r.Append($"/// <summary>Get the configured mock value for {methodInfo.Method.Name} Method");
                r.AppendLine("</summary>");
                r.AppendLine("internal bool Result { get; private set;}");
                r.Append($"/// <summary>Get the fail message for {methodInfo.Method.Name} Method");
                r.AppendLine("</summary>");
                r.AppendLine("internal string ErrorMessage { get; private set;}");
                r.AppendLine();
                r.Append("/// <summary>Pass the method execult with no result");
                r.AppendLine("</summary>");
                r.AppendLine("public void Pass()");
                //Method defination starts here
                r.AppendLine("{");
                r.AppendLine("Result = true;");
                //Method defination ends here
                r.AppendLine("}");
                r.Append("/// <summary>Fail the method execult with exception");
                r.AppendLine("</summary>");
                r.AppendLine("public void Fail(string errorMessage)");
                //Method defination starts here
                r.AppendLine("{");
                r.AppendLine("Result = false;");
                r.AppendLine("ErrorMessage = errorMessage;");
                //Method defination ends here
                r.AppendLine("}");
            }
            //Class defination ends here
            r.AppendLine("}");
            return r.ToString();
        }
        public static string GenerateMethodWithParamBehaviourClass(MethodGenerator methodInfo)
        {
            var r = new StringBuilder();
            r.Append($"/// <summary>set the expected behaviour for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"public class {methodInfo.Method.Name}WithParamExpectation");
            //Class defination starts here
            r.AppendLine("{");
            r.AppendLine();
            if (methodInfo.HasReturnType())
            {
                r.Append($"/// <summary>Get the configured mock value for {methodInfo.Method.Name} Method");
                r.AppendLine("</summary>");
                r.AppendLine($"internal {methodInfo.ReturnType} Result;");
                r.Append($"/// <summary>Get the configured mock Func for {methodInfo.Method.Name} Method");
                r.AppendLine("</summary>");
                r.AppendLine($"internal Func<{methodInfo.GetArgsTypes().WithWrappers("", ",")}{methodInfo.ReturnType}> FuncResult {{ get; private set;}}");
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
            }
            else
            {
                r.Append($"/// <summary>Get the configured mock value for {methodInfo.Method.Name} Method");
                r.AppendLine("</summary>");
                r.AppendLine("internal bool Result;");
                r.Append($"/// <summary>Get the fail message for {methodInfo.Method.Name} Method");
                r.AppendLine("</summary>");
                r.AppendLine("internal string ErrorMessage;");
                r.AppendLine();
                r.Append("/// <summary>Pass the method execult with no result");
                r.AppendLine("</summary>");
                r.AppendLine("public void Pass()");
                r.AppendLine("{");
                r.AppendLine("Result = true;");
                r.AppendLine("}");
                r.Append("/// <summary>Fail the method execult with exception");
                r.AppendLine("</summary>");
                r.AppendLine("public void Fail(string errorMessage)");
                r.AppendLine("{");
                r.AppendLine("Result = false;");
                r.AppendLine("ErrorMessage = errorMessage;");

            }
            r.AppendLine("}");

            r.AppendLine("}");
            return r.ToString();
        }

        static string GetKey(string args) => args.Replace(",", "_");
        static bool HasMethodsWithParams(MethodGenerator methodInfo) => Context.ActionMethods.Any(x => x.Method.Name == methodInfo.Method.Name && !x.GetArgs().IsEmpty());

    }
}