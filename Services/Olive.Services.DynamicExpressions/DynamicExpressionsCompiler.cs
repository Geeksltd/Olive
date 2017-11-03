using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Olive.Framework.Services
{
    class DynamicExpressionsCompiler<T>
    {
        Type ListType;
        IEnumerable<T> List;

        static Dictionary<string, MethodInfo> WhereCache = new Dictionary<string, MethodInfo>();

        static Dictionary<Type, Dictionary<string, MethodInfo>> SelectCache = new Dictionary<Type, Dictionary<string, MethodInfo>>();

        /// <summary>
        /// Creates a new DynamicExpressionsCompiler instance.
        /// </summary>
        public DynamicExpressionsCompiler(IEnumerable<T> list)
            : this(list, typeof(T))
        {
        }

        /// <summary>
        /// Creates a new DynamicExpressionsCompiler instance.
        /// </summary>
        public DynamicExpressionsCompiler(IEnumerable<T> list, Type listType)
        {
            List = list;
            ListType = listType;
        }

        string FullTypeName
        {
            get
            {
                return ListType.FullName;
            }
        }

        internal IEnumerable<T> Where(string criteria)
        {
            var key = ListType.FullName + "|" + criteria;

            lock (WhereCache)
            {
                if (!WhereCache.ContainsKey(key))
                {
                    var method = CompileMethod<T>(criteria, "each");
                    WhereCache.Add(key, method);
                }
            }

            var arg = List.Cast(ListType);

            var result = WhereCache[key].Invoke(null, new object[] { arg });

            return (result as IEnumerable).Cast<T>();
        }

        internal IEnumerable<K> Select<K>(string query)
        {
            Dictionary<string, MethodInfo> pool;

            lock (SelectCache)
            {
                if (SelectCache.ContainsKey(typeof(K)))
                    pool = SelectCache[typeof(K)];
                else
                {
                    pool = new Dictionary<string, MethodInfo>();
                    SelectCache.Add(typeof(K), pool);
                }
            }

            lock (pool)
            {
                if (!pool.ContainsKey(query))
                {
                    var method = CompileMethod<K>(condition: null, expression: query);
                    pool.Add(query, method);
                }
            }

            if (List.None())
            {
                return new List<K>();
            }

            //var actualT = List.First().GetType(); // pool[query].GetParameters().First().ParameterType.GetGenericArguments().First();
            //var listTType = typeof(System.Collections.Generic.List<>).MakeGenericType(actualT);
            //var args = listTType.CreateInstance() as IList;
            ////var args = new List<T>();
            //foreach(var item in List)
            //{
            //    args.Add(item);
            //}
            ////var actualT = pool[query].GetParameters().First().ParameterType.GetGenericArguments().First()


            var arg = List.Cast(ListType);

            var result = pool[query].Invoke(null, new object[] { arg });

            return (result as IEnumerable).Cast<K>();
        }

        string CreateCode<K>(string condition, string expression)
        {
            var r = new StringBuilder();

            var namespaces = new[] { ListType, typeof(K) }.Select(t => t.Namespace)
                .Concat(new[] { "System", "System.Linq", "System.Collections", "System.Collections.Generic" }).Distinct();

            foreach (var n in namespaces)
                r.AddFormattedLine("using {0};", n);

            r.AppendLine("public static class Class");
            r.AppendLine("{");

            r.AddFormattedLine("public static IEnumerable<{1}> Run(IEnumerable<{0}> list)", FullTypeName, typeof(K).FullName);
            r.AppendLine("{");

            r.AppendLine("foreach (var each in list)");
            r.AppendLine("{");

            if (condition.HasValue())
                r.AddFormattedLine("if ({0})", condition);

            r.AddFormattedLine("yield return {0};", expression);

            r.AppendLine("}");

            //r.AddFormattedLine("return list.{0}(each => {1});", method, expression);
            r.AppendLine("}");

            r.AppendLine("}"); // Class

            return r.ToString();
        }

        MethodInfo CompileMethod<K>(string condition, string expression)
        {
            var code = CreateCode<K>(condition, expression);

            var compiler = new Compiler();
            compiler.Reference(ListType, ignoreFailedAssemblies: true);

            var type = compiler.CompileClass(code);

            return type.GetMethod("Run");
        }
    }
}