using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
            r.AppendLine("internal bool Enabled { get; set; }");
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
            //Class defination ends here
            r.AppendLine("}");
            foreach (var method in Context.ActionMethods.Distinct(x => x.Method.Name))
                r.AppendLine(GenerateApiBehaviourClass(method));

            //Namespace defination ends here
            r.AppendLine("}");
            return new CSharpFormatter(r.ToString()).Format();
        }
        public static string GenerateApiBehaviourClass(MethodGenerator methodInfo)
        {
            var r = new StringBuilder();
            r.Append($"/// <summary>set the expected behaviour for {ClassName} method {methodInfo.Method.Name}");
            r.AppendLine("</summary>");
            r.AppendLine($"public partial class {ClassName}Behaviour");
            //Class defination starts here
            r.AppendLine("{");
            //define properties for behaviours here
            r.AppendLine();
            r.Append($"/// <summary>This will holds the expectations for all overloads of {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"Dictionary<string,{methodInfo.Method.Name}Expectation> {methodInfo.Method.Name}Expectations = new Dictionary<string,{methodInfo.Method.Name}Expectation>();");
            r.AppendLine();
            //define method without parameters
            r.Append($"/// <summary>Set the mock expectation for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.Append($"public {methodInfo.Method.Name}Expectation {methodInfo.Method.Name}()");
            r.Append(" => ");
            r.AppendLine($"{methodInfo.Method.Name}Expectations[\"\"]=new {methodInfo.Method.Name}Expectation();");


            foreach (var overload in Context.ActionMethods.Where(x => x.Method.Name == methodInfo.Method.Name && !x.GetArgs().IsEmpty()))
            {

                //define method with parameters
                r.Append($"/// <summary>Set the mock expectation for {overload.Method.Name} Method");
                r.AppendLine("</summary>");
                r.AppendLine($"public {overload.Method.Name}Expectation {overload.Method.Name}({overload.GetArgs()})");
                r.AppendLine("{");
                r.AppendLine($"var key=\"{GetKey(overload.GetArgsTypes())}\";");
                r.AppendLine($"return {overload.Method.Name}Expectations[key]=new {overload.Method.Name}Expectation();");
                r.AppendLine("}");
            }
            //generate the result methods 
            r.Append($"/// <summary>Get the expected result for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.Append($"internal {methodInfo.ReturnType} {methodInfo.Method.Name}Result()");
            r.Append(" => ");
            r.AppendLine($"{methodInfo.Method.Name}Expectations[\"\"]?.Result;");
            foreach (var overload in Context.ActionMethods.Where(x => x.Method.Name == methodInfo.Method.Name && !x.GetArgs().IsEmpty()))
            {

                //define result method with parameters
                r.Append($"/// <summary>Get the expected result for {overload.Method.Name} Method");
                r.AppendLine("</summary>");
                r.AppendLine($"internal {overload.ReturnType} {overload.Method.Name}Result({overload.GetArgs()})");
                r.AppendLine("{");
                r.AppendLine($"var key=\"{GetKey(overload.GetArgsTypes())}\";");

                r.AppendLine($"return {overload.Method.Name}Expectations[key].Result;");
                r.AppendLine("}");
            }
            r.AppendLine(GenerateMethodBehaviourClass(methodInfo));
            //Class defination ends here
            r.AppendLine("}");

            return r.ToString();
        }
        public static string GenerateMethodBehaviourClass(MethodGenerator methodInfo)
        {
            var r = new StringBuilder();
            r.Append($"/// <summary>set the expected behaviour for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"public class {methodInfo.Method.Name}Expectation");
            //Class defination starts here
            r.AppendLine("{");
            r.AppendLine();
            r.Append($"/// <summary>Get teh configured mock value for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"internal {methodInfo.ReturnType} Result {{ get; private set;}}");
            r.AppendLine();
            r.Append($"/// <summary>Set the mock returned value for {methodInfo.Method.Name} Method");
            r.AppendLine("</summary>");
            r.AppendLine($"public void Returns({methodInfo.ReturnType} expectedResult)");
            //Method defination starts here
            r.AppendLine("{");
            r.AppendLine("Result = expectedResult;");
            //Method defination ends here
            r.AppendLine("}");
            //Class defination ends here
            r.AppendLine("}");

            return r.ToString();
        }
        static string GetKey(string args) => args.Replace(",", "_");
    }
}
