using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Olive.Entities.Data
{
    static partial class DataProviderMetaDataGenerator
    {
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
            var sourceCode = $"using System;\r\nusing Olive.Entities;{code}";
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            string assemblyName = Path.GetRandomFileName();
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(IEntity).Assembly.Location),
                MetadataReference.CreateFromFile(type.Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Guid).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Console.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")),
            };

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                    throw new Exception("Failed to create property accessors type.");

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
            var propertyType = property.PropertyInfo.PropertyType;
            var isBlob = propertyType.IsA<Blob>();
            var fullTypeName = $"{type.Namespace}.{type.Name}";

            string GetValuePart()
            {
                if (isBlob) return "new Blob { FileName = value as string }";

                return $"{GetCastingType(propertyType)}value";
            }

            string getSetBody()
            {
                if (property.IsOriginalId)
                    return "throw new InvalidOperationException(\"Cannot set the original Id in this way.\");";

                if(property.IsDeleted)
                    return  $@"var obj = ({fullTypeName})entity;
                        if ((Boolean)value)
                            SoftDeleteAttribute.MarkDeleted(obj);
                        else
                            SoftDeleteAttribute.UnMark(obj);";

                return $@"var obj = ({fullTypeName})entity;
                        obj.{property.PropertyInfo.Name} = {GetValuePart()};";
            }

            return $@"
                public class {property.PropertyInfo.Name}Accessor : IPropertyAccessor
                {{
                    public object Get(IEntity entity) => (({fullTypeName})entity).{property.PropertyInfo.Name}{"?.FileName".OnlyWhen(isBlob)};

                    public void Set(IEntity entity, object value)
                    {{
                        {getSetBody()}
                    }}
                }}";
        }

        static string GetCastingType(Type type)
        {
            if (type.IsNullable())
                return GetCastingType(type.GenericTypeArguments[0]);//.WithSuffix("?");

            if(type.IsA<Double>()) return $"({type.Name})(decimal)";
            
            return $"({type.Name})";
        }
    }
}
