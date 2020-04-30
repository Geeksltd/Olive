using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Olive.Entities.Data
{
    static partial class DataProviderMetaDataGenerator
    {
        static ConcurrentDictionary<Assembly, List<PortableExecutableReference>> Cache =
            new ConcurrentDictionary<Assembly, List<PortableExecutableReference>>();

        static IPropertyData[] SetAccessors(this IEnumerable<PropertyData> @this, Type type)
        {
            var list = @this.ToList();

            var code = "";

            list.ForEach(p => code += GetAccessorClass(type, p));

            var accessores = GetAccessorTypes(type, code);

            list.ForEach(p => p.Accessor = accessores[p.PropertyInfo.Name + "Accessor"]);

            return list.ToArray();
        }

        static Dictionary<string, IPropertyAccessor> GetAccessorTypes(Type type, string code)
        {
            var sourceCode = $"using System;\r\nusing Olive.Entities;\r\nusing System.Data;{code}";
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            var assemblyName = Path.GetRandomFileName();

            var references2 = Cache.GetOrAdd(type.Assembly, assembly =>
            {
                var references = new List<PortableExecutableReference>
                    {
                        MetadataReference.CreateFromFile(typeof(IEntity).Assembly.Location),
                        MetadataReference.CreateFromFile(type.Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Guid).Assembly.Location),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Data.Common.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Console.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")),
                    };

                foreach (var item in type.Assembly.GetReferencedAssemblies())
                    references.Add(MetadataReference.CreateFromFile(Assembly.Load(item).Location));
                return references;
            });

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references2,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    var error = new Exception("DataProviderMetaDataGenerator failed." +
                       emitResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error)
                       .Select(v => v.GetMessage()).Distinct().ToLinesString().WithPrefix(Environment.NewLine));
                    Console.WriteLine(error.Message);
                    throw error;
                }

                stream.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(stream.ToArray());

                var result = new Dictionary<string, IPropertyAccessor>();

                foreach (var accessorType in assembly.GetTypes())
                    result.Add(accessorType.Name, (IPropertyAccessor)Activator.CreateInstance(accessorType));

                return result;
            }
        }

        static string GetAccessorClass(Type type, PropertyData property)
        {
            if (property.CustomDataConverterClassName.HasValue())
                return new AccessorClassGenerator(type, property).Generate();
            else
                return new BasicAccessorClassGenerator(type, property).Generate();
        }

        class BasicAccessorClassGenerator : AccessorClassGenerator
        {
            public BasicAccessorClassGenerator(Type type, PropertyData property) : base(type, property) { }

            public override string Generate()
            {
                return $@"
                    public class {Property.PropertyInfo.Name}Accessor : IPropertyAccessor
                    {{
                        public object Get(IEntity entity) => (({FullTypeName})entity).{Property.PropertyInfo.Name}{"?.FileName".OnlyWhen(IsBlob)};

                        public void Set(IEntity entity, object value)
                        {{
                            {GetSetBody()}
                        }}

                        public void Set(IEntity entity, IDataReader reader, int index)
                        {{
                            {GetSetBodyFromReader()}
                        }}
                    }}";
            }

            string GetValuePart()
            {
                if (IsBlob) return "new Blob { FileName = value == DBNull.Value ? null : value as string }";

                return $"value == DBNull.Value ? {GetCastingType(PropertyType)}null : ".OnlyWhen(Type.IsNullable()) +
                    $"{GetCastingType(PropertyType)}value";
            }

            string GetSetBody()
            {
                if (Property.IsOriginalId)
                    return "throw new InvalidOperationException(\"Cannot set the original Id in this way.\");";

                if (Property.IsDeleted)
                    return $@"var obj = ({FullTypeName})entity;
                        if ((Boolean)value)
                            SoftDeleteAttribute.MarkDeleted(obj);
                        else
                            SoftDeleteAttribute.UnMark(obj);";

                return $@"var obj = ({FullTypeName})entity;
                        obj.{Property.PropertyInfo.Name} = {GetValuePart()};";
            }

            string GetValuePartFromReader()
            {
                if (IsBlob) return "new Blob { FileName = reader.IsDBNull(index) ? null : reader.GetString(index) }";

                return FindGetValueExpression(PropertyType);
            }

            string GetSetBodyFromReader()
            {
                if (Property.IsOriginalId)
                    return "throw new InvalidOperationException(\"Cannot set the original Id in this way.\");";

                if (Property.IsDeleted)
                    return $@"var obj = ({FullTypeName})entity;
                        if (reader.GetBoolean(index))
                            SoftDeleteAttribute.MarkDeleted(obj);
                        else
                            SoftDeleteAttribute.UnMark(obj);";

                return $@"var obj = ({FullTypeName})entity;
                        obj.{Property.PropertyInfo.Name} = {GetValuePartFromReader()};";
            }

            string GetCastingType(Type type, bool ignoreParentheses = false)
            {
                if (type.IsNullable())
                    return $"({GetCastingType(type.GenericTypeArguments[0], ignoreParentheses: true)}?)";

                if (type.IsA<double>()) return ignoreParentheses ? $"{type.Name}?)(decimal" : $"({type.Name})(decimal)";

                return ignoreParentheses ? type.Name : $"({type.Name})";
            }

            string FindGetValueExpression(Type type)
            {
                if (type.IsNullable())
                    return "reader.IsDBNull(index) ? (" +
                        GetCastingType(type.GenericTypeArguments[0], ignoreParentheses: true) +
                        $"?)null : {FindGetValueExpression(type.GenericTypeArguments[0])}";

                if (type.IsA<bool>()) return "reader.GetBoolean(index)";
                if (type.IsA<byte>()) return "reader.GetByte(index)";
                if (type.IsA<char>()) return "reader.GetChar(index)";
                if (type.IsA<DateTime>()) return "reader.GetDateTime(index)";
                if (type.IsA<decimal>()) return "reader.GetDecimal(index)";
                if (type.IsA<double>()) return "(double) reader.GetDecimal(index)";
                if (type.IsA<float>()) return "reader.GetFloat(index)";
                if (type.IsA<short>()) return "reader.GetInt16(index)";
                if (type.IsA<int>()) return "reader.GetInt32(index)";
                if (type.IsA<long>()) return "reader.GetInt64(index)";
                if (type.IsA<string>()) return "reader.IsDBNull(index) ? null : reader.GetString(index)";
                if (type.IsA<Guid>()) return "reader.GetGuid(index)";

                return $"{GetCastingType(type)} reader[index]";
            }
        }

        class AccessorClassGenerator
        {
            readonly protected Type Type;
            readonly protected PropertyData Property;
            readonly protected Type PropertyType;
            readonly protected bool IsBlob;
            readonly protected string FullTypeName;

            public AccessorClassGenerator(Type type, PropertyData property)
            {
                Type = type;
                Property = property;
                PropertyType = property.PropertyInfo.PropertyType;
                IsBlob = PropertyType.IsA<Blob>();
                FullTypeName = $"{type.Namespace}.{type.Name}";
            }

            public virtual string Generate()
            {
                return $@"
                    public class {Property.PropertyInfo.Name}Accessor : IPropertyAccessor
                    {{
                        {Property.CustomDataConverterClassName} Converter = new {Property.CustomDataConverterClassName}();

                        public object Get(IEntity entity) => Converter.ConvertFrom((({FullTypeName})entity).{Property.PropertyInfo.Name});

                        public void Set(IEntity entity, object value)
                        {{
                            var obj = ({FullTypeName})entity;
                            obj.{Property.PropertyInfo.Name} = Converter.ConvertTo(value);
                        }}

                        public void Set(IEntity entity, IDataReader reader, int index) => Set(entity, reader[index]);
                    }}";
            }
        }
    }
}
