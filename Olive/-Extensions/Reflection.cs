using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Olive
{
    partial class OliveExtensions
    {
        static ConcurrentDictionary<Tuple<Type, Type>, bool> TypeImplementsCache = new ConcurrentDictionary<Tuple<Type, Type>, bool>();

        static SortedDictionary<string, object> CacheData = new SortedDictionary<string, object>();

        static ConcurrentDictionary<Assembly, ConcurrentDictionary<Type, IEnumerable<Type>>> SubTypesCache = new ConcurrentDictionary<Assembly, ConcurrentDictionary<Type, IEnumerable<Type>>>();

        static ConcurrentDictionary<Type, string> ProgrammingNameCache = new ConcurrentDictionary<Type, string>();

        static ConcurrentDictionary<Type, string> AssemblyQualifiedNameCache = new ConcurrentDictionary<Type, string>();

        public delegate T Method<out T>();

        /// <summary>
        /// Gets all parent types hierarchy for this type.
        /// </summary>
        public static IEnumerable<Type> GetParentTypes(this Type type)
        {
            var result = new List<Type>();

            for (var @base = type.BaseType; @base != null; @base = @base.BaseType)
                result.Add(@base);

            return result;
        }

        /// <summary>
        /// Determines whether this type inherits from a specified base type, either directly or indirectly.
        /// </summary>
        public static bool InhritsFrom(this Type type, Type baseType)
        {
            if (baseType == null)
                throw new ArgumentNullException(nameof(baseType));

            if (baseType.IsInterface)
                return type.Implements(baseType);

            return type.GetParentTypes().Contains(baseType);
        }

        public static bool Implements<T>(this Type type) => Implements(type, typeof(T));

        public static bool IsA<T>(this Type type) => typeof(T).IsAssignableFrom(type);

        public static bool IsA(this Type thisType, Type type) => type.IsAssignableFrom(thisType);

        public static bool References(this Assembly assembly, Assembly anotherAssembly)
        {
            if (assembly == null) throw new NullReferenceException("assembly should not be null.");
            if (anotherAssembly == null) throw new ArgumentNullException(nameof(anotherAssembly));

            return assembly.GetReferencedAssemblies().Any(each => each.FullName.Equals(anotherAssembly.FullName));
        }

        public static string GetDisplayName(this Type input)
        {
            var displayName = input.Name;

            for (int i = displayName.Length - 1; i >= 0; i--)
            {
                if (displayName[i] == char.ToUpper(displayName[i]))
                    if (i > 0)
                        displayName = displayName.Insert(i, " ");
            }

            return displayName;
        }

        public static IEnumerable<Type> WithAllParents(this Type @this)
        {
            yield return @this;

            if (@this.BaseType != null)
                foreach (var p in @this.BaseType.WithAllParents()) yield return p;
        }

        /// <summary>
        /// Retuns the name of this type in the same way that is used in C# programming.
        /// </summary>
        public static string GetCSharpName(this Type type, bool includeNamespaces = false)
        {
            if (type.GetGenericArguments().None()) return type.Name;

            return type.Name.TrimAfter("`", trimPhrase: true) + "<" +
                type.GetGenericArguments().Select(t => t.GetCSharpName(includeNamespaces)).ToString(", ") + ">";
        }

        public static bool Implements(this Type type, Type interfaceType)
        {
            if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));

            var key = Tuple.Create(type, interfaceType);

            return TypeImplementsCache.GetOrAdd(key, t =>
            {
                if (!interfaceType.IsInterface)
                    throw new ArgumentException($"The provided value for interfaceType, {interfaceType.FullName} is not an interface type.");

                if (t.Item1 == t.Item2) return true;

                var implementedInterface = t.Item1.GetInterface(t.Item2.FullName, ignoreCase: false);

                if (implementedInterface == null) return false;
                else return implementedInterface.FullName == t.Item2.FullName;
            });
        }

        /// <summary>
        /// Gets the value of this property on the specified object.
        /// </summary>
        public static object GetValue(this PropertyInfo property, object @object)
        {
            try
            {
                return property.GetValue(@object, null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not get the value of property '{property.DeclaringType.Name}.{property.Name}' " +
                    "on the specified instance: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Set the value of this property on the specified object.
        /// </summary>
        public static void SetValue(this PropertyInfo property, object @object, object value) => property.SetValue(@object, value, null);

        /// <summary>
        /// Adds the specified types pair to this type dictionary.
        /// </summary>
        public static void Add<T, K>(this IDictionary<Type, Type> typeDictionary) => typeDictionary.Add(typeof(T), typeof(K));

        /// <summary>
        /// Creates the instance of this type.
        /// </summary>
        public static object CreateInstance(this Type type, params object[] constructorParameters) =>
            Activator.CreateInstance(type, constructorParameters);

        /// <summary>
        /// Determines whether it has a specified attribute applied to it.
        /// </summary>
        public static bool Defines<TAttribute>(this MemberInfo member, bool inherit = true) where TAttribute : Attribute =>
            member.IsDefined(typeof(TAttribute), inherit);

        /// <summary>
        /// Creates the instance of this type casted to the specified type.
        /// </summary>
        public static TCast CreateInstance<TCast>(this Type type, params object[] constructorParameters) =>
            (TCast)Activator.CreateInstance(type, constructorParameters);

        public static T Cache<T>(this MethodBase method, object[] arguments, Method<T> methodBody) where T : class =>
            Cache<T>(method, null, arguments, methodBody);

        /// <summary>
        /// Determines if this type is a nullable of something.
        /// </summary>
        public static bool IsNullable(this Type type)
        {
            if (!type.IsGenericType) return false;

            if (type.GetGenericTypeDefinition() != typeof(Nullable<>)) return false;

            return true;
        }

        public static T Cache<T>(this MethodBase method, object instance, object[] arguments, Method<T> methodBody) where T : class
        {
            var key = method.DeclaringType.GUID + ":" + method.Name;
            if (instance != null)
                key += instance.GetHashCode() + ":";

            arguments?.Do(arg => key += arg.GetHashCode() + ":");

            if (CacheData[key] == null)
            {
                var result = methodBody?.Invoke();
                CacheData[key] = result;
                return result;
            }

            return CacheData[key] as T;
        }

        public static bool Is<T>(this PropertyInfo property, string propertyName)
        {
            var type1 = property.DeclaringType;
            var type2 = typeof(T);

            if (type1.IsA(type2) || type2.IsA(type1))
                return property.Name == propertyName;
            else
                return false;
        }

        /// <summary>
        /// Determines whether this type is static.
        /// </summary>
        public static bool IsStatic(this Type type) => type.IsAbstract && type.IsSealed;

        public static bool IsExtensionMethod(this MethodInfo method) =>
            method.GetCustomAttributes<System.Runtime.CompilerServices.ExtensionAttribute>(inherit: false).Any();

        // /// <summary>
        // /// Gets all defined attributes of the specified type.
        // /// </summary>
        // public static TAttribute[] GetCustomAttributes1<TAttribute>(this MemberInfo member, bool inherit = true) where TAttribute : Attribute
        // {
        //    member.GetCustomAttributes<TAttribute>()

        //    var result = member.GetCustomAttributes(typeof(TAttribute), inherit);
        //    if (result == null) return new TAttribute[0];
        //    else return result.Cast<TAttribute>().ToArray();
        // }

        #region Sub-Types

        /// <summary>
        /// Gets all types in this assembly that are directly inherited from a specified base type.
        /// </summary>
        public static IEnumerable<Type> GetSubTypes(this Assembly assembly, Type baseType)
        {
            var cache = SubTypesCache.GetOrAdd(assembly, a => new ConcurrentDictionary<Type, IEnumerable<Type>>());

            return cache.GetOrAdd(baseType, bt =>
            {
                try
                {
                    return assembly.GetTypes().Where(t => t.BaseType == bt).ToArray();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    throw new Exception("Could not load the types of the assembly '{0}'. Type-load exceptions: {1}".FormatWith(assembly.FullName,
                        ex.LoaderExceptions.Select(e => e.Message).Distinct().ToString(" | ")));
                }
            });
        }

        #endregion

        /// <summary>
        /// Gets the full programming name of this type. Unlike the standard FullName property, it handles Generic types properly.
        /// </summary>
        public static string GetProgrammingName(this Type type) =>
            ProgrammingNameCache.GetOrAdd(type, x => GetProgrammingName(x, useGlobal: false));

        /// <summary>
        /// Gets the full programming name of this type. Unlike the standard FullName property, it handles Generic types properly.
        /// </summary>
        public static string GetProgrammingName(this Type type, bool useGlobal, bool useNamespace = true, bool useNamespaceForParams = true, bool useGlobalForParams = false)
        {
            if (type.GetGenericArguments().Any())
            {
                return "global::".OnlyWhen(useGlobal && type.FullName != null) +
                    "{0}{1}<{2}>".FormatWith(
                    type.Namespace.OnlyWhen(useNamespace).WithSuffix("."),
                    type.Name.Remove(type.Name.IndexOf('`')),
                    type.GetGenericArguments().Select(t => t.GetProgrammingName(useGlobalForParams, useNamespaceForParams, useNamespaceForParams, useGlobalForParams)).ToString(", "));
            }
            else
            {
                if (type.FullName == null)
                {
                    // Generic parameter name:
                    return type.Name.TrimEnd("&");
                }

                return "global::".OnlyWhen(useGlobal) + type.Namespace.OnlyWhen(useNamespace).WithSuffix(".") + type.Name.Replace("+", ".").TrimEnd("&");
            }
        }

        /// <summary>
        /// Determines if this type is a generic class  of the specified type.
        /// </summary>
        public static bool IsGenericOf(this Type type, Type genericType, params Type[] genericParameters)
        {
            if (!type.IsGenericType) return false;

            if (type.GetGenericTypeDefinition() != genericType) return false;

            var args = type.GetGenericArguments();

            if (args.Length != genericParameters.Length) return false;

            for (var i = 0; i < args.Length; i++)
                if (args[i] != genericParameters[i]) return false;

            return true;
        }

        internal static bool IsAnyOf(this Type type, params Type[] types)
        {
            if (type == null) return types.Any(x => x == null);

            return types.Contains(type);
        }

        public static string GetCachedAssemblyQualifiedName(this Type type) =>
            AssemblyQualifiedNameCache.GetOrAdd(type, x => x.AssemblyQualifiedName);

        public static MemberInfo GetPropertyOrField(this Type type, string name) =>
            type.GetProperty(name) ?? (MemberInfo)type.GetField(name);

        public static IEnumerable<MemberInfo> GetPropertiesAndFields(this Type type, BindingFlags flags) =>
            type.GetProperties(flags).Cast<MemberInfo>().Concat(type.GetFields(flags));

        public static Type GetPropertyOrFieldType(this MemberInfo member) =>
            (member as PropertyInfo)?.PropertyType ?? (member as FieldInfo)?.FieldType;

        static IEnumerable<Assembly> GetReferencingAssemblies(Assembly anotherAssembly) =>
            AppDomain.CurrentDomain.GetAssemblies().Where(assebly => assebly.References(anotherAssembly));

        public static IEnumerable<Type> SelectTypesByAttribute<T>(this Assembly assembly, bool inherit) where T : Attribute =>
            assembly.GetExportedTypes().Where(t => t.IsDefined(typeof(T), inherit));

        /// <summary>
        /// Gets all types in the current appDomain which implement this interface.
        /// </summary>
        public static List<Type> FindImplementerClasses(this Type interfaceType)
        {
            if (!interfaceType.IsInterface) throw new InvalidOperationException(interfaceType.GetType().FullName + " is not an Interface.");

            var result = new List<Type>();

            foreach (var assembly in GetReferencingAssemblies(interfaceType.Assembly))
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type == interfaceType) continue;
                        if (!type.IsClass) continue;

                        if (type.Implements(interfaceType))
                            result.Add(type);
                    }
                }
                catch
                {
                    // No logging is needed
                    // Can't load assembly
                }
            }

            return result;
        }

        public static object GetObjectByPropertyPath(this Type type, object instance, string propertyPath)
        {
            if (propertyPath.Contains("."))
            {
                var directProperty = type.GetProperty(propertyPath.TrimAfter("."));

                if (directProperty == null)
                    throw new Exception(type.FullName + " does not have a property named '" + propertyPath.TrimAfter(".") + "'");

                var associatedObject = directProperty.GetValue(instance);
                if (associatedObject == null) return null;

                var remainingPath = propertyPath.TrimStart(directProperty.Name + ".");
                return associatedObject.GetType().GetObjectByPropertyPath(associatedObject, remainingPath);
            }
            else
            {
                return type.GetProperty(propertyPath).GetValue(instance);
            }
        }

        /// <summary>
        /// Creates a new thread and copies the current Culture and UI Culture.
        /// </summary>
        public static Thread CreateNew(this Thread thread, Action threadStart) => CreateNew(thread, threadStart, null);

        /// <summary>
        /// Creates a new thread and copies the current Culture and UI Culture.
        /// </summary>
        public static Thread CreateNew(this Thread thread, Action threadStart, Action<Thread> initializer)
        {
            var result = new Thread(new ThreadStart(threadStart));

            initializer?.Invoke(result);

            return result;
        }

        /// <summary>
        /// Gets the default value for this type. It's equivalent to default(T).
        /// </summary>
        public static object GetDefaultValue(this Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        /// <summary>
        /// If it specifies DisplayNameAttribute the value from that will be returned.
        /// Otherwise it returns natural English literal text for the name of this member.
        /// For example it coverts "ThisIsSomething" to "This is something".
        /// </summary>
        public static string GetDisplayName(this MemberInfo member)
        {
            var byAttribute = member.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>()?.DisplayName;
            return byAttribute.Or(() => member.Name.ToLiteralFromPascalCase());
        }

        /// <summary>
        /// Determine whether this property is static.
        /// </summary>
        public static bool IsStatic(this PropertyInfo property) => (property.GetGetMethod() ?? property.GetSetMethod()).IsStatic;

        public static TTArget GetTargetOrDefault<TTArget>(this WeakReference<TTArget> reference)
            where TTArget : class
        {
            if (reference == null) return null;

            if (reference.TryGetTarget(out var result)) return result;
            return null;
        }

        internal static WeakReference<T> GetWeakReference<T>(this T item) where T : class
            => new WeakReference<T>(item);

        public static object GetValue(this MemberInfo classMember, object obj)
        {
            if (classMember is PropertyInfo asProp) return asProp.GetValue(obj);

            if (classMember is FieldInfo asField) return asField.GetValue(obj);

            if (classMember is MethodInfo asMethod) return asMethod.Invoke(obj, new object[0]);

            throw new Exception("GetValue() is not implemented for " + classMember?.GetType().Name);
        }
    }
}
