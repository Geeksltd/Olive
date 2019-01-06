using System;
using System.Collections;
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
        static ConcurrentDictionary<Assembly, ConcurrentDictionary<Type, IEnumerable<Type>>> DescendantsTypesCache = new ConcurrentDictionary<Assembly, ConcurrentDictionary<Type, IEnumerable<Type>>>();

        static ConcurrentDictionary<Type, string> ProgrammingNameCache = new ConcurrentDictionary<Type, string>();

        static ConcurrentDictionary<Type, string> AssemblyQualifiedNameCache = new ConcurrentDictionary<Type, string>();

        static readonly Dictionary<Type, string> CSharpTypeAliases =
            new Dictionary<Type, string>
            {
                { typeof(int), "int" },
                { typeof(short), "short" },
                { typeof(long), "long" },
                { typeof(float), "float" },
                { typeof(double), "double" },
                { typeof(decimal), "decimal" },
                { typeof(bool), "bool" },
                { typeof(char), "char" },
                { typeof(string), "string" },

                { typeof(int?), "int?" },
                { typeof(short?), "short?" },
                { typeof(long?), "long?" },
                { typeof(float?), "float?" },
                { typeof(double?), "double?" },
                { typeof(decimal?), "decimal?" },
                { typeof(bool?), "bool?" },
                { typeof(char?), "char?" }
            };

        public delegate T Method<out T>();

        /// <summary>
        /// Gets all parent types hierarchy for this type.
        /// </summary>
        public static IEnumerable<Type> GetParentTypes(this Type @this)
        {
            var result = new List<Type>();

            for (var @base = @this.BaseType; @base != null; @base = @base.BaseType)
                result.Add(@base);

            return result;
        }

        /// <summary>
        /// Determines whether this type inherits from a specified base type, either directly or indirectly.
        /// </summary>
        public static bool InhritsFrom(this Type @this, Type baseType)
        {
            if (baseType == null)
                throw new ArgumentNullException(nameof(baseType));

            if (baseType.IsInterface)
                return @this.Implements(baseType);

            return @this.GetParentTypes().Contains(baseType);
        }

        public static bool Implements<T>(this Type @this) => Implements(@this, typeof(T));

        [EscapeGCop("I am the solution to this GCop warning")]
        public static bool IsA<T>(this Type @this) => typeof(T).IsAssignableFrom(@this);

        public static MethodInfo GetGenericMethod(this Type @this, string methodName, params Type[] genericTypes)
        {
            return @this.GetMethod(methodName).MakeGenericMethod(genericTypes);
        }

        [EscapeGCop("I am the solution to this GCop warning")]
        public static bool IsA(this Type @this, Type type) => type.IsAssignableFrom(@this);

        public static bool References(this Assembly @this, Assembly anotherAssembly)
        {
            if (@this == null) throw new NullReferenceException("assembly should not be null.");
            if (anotherAssembly == null) throw new ArgumentNullException(nameof(anotherAssembly));

            return @this.GetReferencedAssemblies().Any(each => each.FullName == anotherAssembly.FullName);
        }

        public static string GetDisplayName(this Type @this)
        {
            var displayName = @this.Name;

            for (var i = displayName.Length - 1; i >= 0; i--)
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
        public static string GetCSharpName(this Type @this, bool includeNamespaces = false)
        {
            if (@this.GetGenericArguments().None()) return @this.Name;

            return @this.Name.RemoveFrom("`") + "<" +
                @this.GetGenericArguments().Select(t => t.GetCSharpName(includeNamespaces)).ToString(", ") + ">";
        }

        public static bool Implements(this Type @this, Type interfaceType)
        {
            if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));

            var key = Tuple.Create(@this, interfaceType);

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
        public static object GetValue(this PropertyInfo @this, object @object)
        {
            try
            {
                return @this.GetValue(@object, null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not get the value of property '{@this.DeclaringType.Name}.{@this.Name}' " +
                    "on the specified instance: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Set the value of this property on the specified object.
        /// </summary>
        public static void SetValue(this PropertyInfo @this, object @object, object value) => @this.SetValue(@object, value, null);

        /// <summary>
        /// Creates the instance of this type.
        /// </summary>
        public static object CreateInstance(this Type @this, params object[] constructorParameters) =>
            Activator.CreateInstance(@this, constructorParameters);

        /// <summary>
        /// Creates the instance of this type.
        /// </summary>
        public static object CreateInstanceWithDI(this Type @this)
        {
            var ctors = @this.GetConstructors();
            if (ctors.HasMany()) throw new Exception("Multiple constructors found for: " + @this.FullName);

            var parameters = ctors.Single().GetParameters()
                .Select(x => Context.Current.ServiceProvider.GetService(x.ParameterType))
                .ToArray();

            return @this.CreateInstance(parameters);
        }

        /// <summary>
        /// Determines whether it has a specified attribute applied to it.
        /// </summary>
        public static bool Defines<TAttribute>(this MemberInfo @this, bool inherit = true) where TAttribute : Attribute =>
            @this.IsDefined(typeof(TAttribute), inherit);

        /// <summary>
        /// Creates the instance of this type casted to the specified type.
        /// </summary>
        public static TCast CreateInstance<TCast>(this Type @this, params object[] constructorParameters) =>
            (TCast)Activator.CreateInstance(@this, constructorParameters);

        public static T Cache<T>(this MethodBase @this, object[] arguments, Method<T> methodBody) where T : class =>
            Cache<T>(@this, null, arguments, methodBody);

        /// <summary>
        /// Determines if this type is a nullable of something.
        /// </summary>
        public static bool IsNullable(this Type @this)
        {
            if (!@this.IsGenericType) return false;

            if (@this.GetGenericTypeDefinition() != typeof(Nullable<>)) return false;

            return true;
        }

        public static T Cache<T>(this MethodBase @this, object instance, object[] arguments, Method<T> methodBody) where T : class
        {
            var key = @this.DeclaringType.GUID + ":" + @this.Name;
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

        public static bool Is<T>(this PropertyInfo @this, string propertyName)
        {
            var type1 = @this.DeclaringType;
            var type2 = typeof(T);

            if (type1.IsA(type2) || type2.IsA(type1))
                return @this.Name == propertyName;
            else
                return false;
        }

        /// <summary>
        /// Determines whether this type is static.
        /// </summary>
        public static bool IsStatic(this Type @this) => @this.IsAbstract && @this.IsSealed;

        public static bool IsExtensionMethod(this MethodInfo @this) =>
            @this.GetCustomAttributes<System.Runtime.CompilerServices.ExtensionAttribute>(inherit: false).Any();

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
        /// Gets all types in this assembly that inherit from a specified base type.
        /// </summary>
        public static IEnumerable<Type> GetSubTypes(this Assembly @this, Type baseType, bool withDescendants = false)
        {
            var bag = withDescendants ? DescendantsTypesCache : SubTypesCache;

            var cache = bag.GetOrAdd(@this, a => new ConcurrentDictionary<Type, IEnumerable<Type>>());

            return cache.GetOrAdd(baseType, bt =>
            {
                try
                {
                    var result = @this.GetTypes().Where(t => t.BaseType == bt).ToArray();
                    if (!withDescendants) return result;
                    return result.Concat(result.SelectMany(x => x.Assembly.GetSubTypes(x, withDescendants))).ToArray();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    throw new Exception("Could not load the types of the assembly '{0}'. Type-load exceptions: {1}".FormatWith(@this.FullName,
                        ex.LoaderExceptions.Select(e => e.Message).Distinct().ToString(" | ")));
                }
            });
        }

        #endregion

        /// <summary>
        /// Gets the full programming name of this type. Unlike the standard FullName property, it handles Generic types properly.
        /// </summary>
        public static string GetProgrammingName(this Type @this) =>
            ProgrammingNameCache.GetOrAdd(@this, x => GetProgrammingName(x, useGlobal: false));

        /// <summary>
        /// Gets the full programming name of this type. Unlike the standard FullName property, it handles Generic types properly.
        /// </summary>
        public static string GetProgrammingName(this Type @this, bool useGlobal, bool useNamespace = true, bool useNamespaceForParams = true, bool useGlobalForParams = false, bool useCSharpAlias = false)
        {
            if (useCSharpAlias && CSharpTypeAliases.TryGetValue(@this, out var alias)) return alias;

            if (@this.GetGenericArguments().Any())
            {
                return "global::".OnlyWhen(useGlobal && @this.FullName != null) +
                    "{0}{1}<{2}>".FormatWith(
                    @this.Namespace.OnlyWhen(useNamespace).WithSuffix("."),
                    @this.Name.Remove(@this.Name.IndexOf('`')),
                    @this.GetGenericArguments().Select(t => t.GetProgrammingName(useGlobalForParams, useNamespaceForParams, useNamespaceForParams, useGlobalForParams, useCSharpAlias)).ToString(", "));
            }
            else
            {
                if (@this.FullName == null)
                {
                    // Generic parameter name:
                    return @this.Name.TrimEnd("&");
                }

                return "global::".OnlyWhen(useGlobal) + @this.Namespace.OnlyWhen(useNamespace).WithSuffix(".") + @this.Name.Replace("+", ".").TrimEnd("&");
            }
        }

        /// <summary>
        /// Determines if this type is a generic class  of the specified type.
        /// </summary>
        public static bool IsGenericOf(this Type @this, Type genericType, params Type[] genericParameters)
        {
            if (!@this.IsGenericType) return false;

            if (@this.GetGenericTypeDefinition() != genericType) return false;

            var args = @this.GetGenericArguments();

            if (args.Length != genericParameters.Length) return false;

            for (var i = 0; i < args.Length; i++)
                if (args[i] != genericParameters[i]) return false;

            return true;
        }

        public static bool IsAnyOf(this Type @this, params Type[] types) => types.Contains(@this);

        public static string GetCachedAssemblyQualifiedName(this Type @this) =>
            AssemblyQualifiedNameCache.GetOrAdd(@this, x => x.AssemblyQualifiedName);

        public static MemberInfo GetPropertyOrField(this Type @this, string name) =>
            @this.GetProperty(name) ?? (MemberInfo)@this.GetField(name);

        public static PropertyInfo GetPropertyOrThrow(this Type @this, string name)
        {
            return @this.GetProperty(name)
                ?? throw new Exception(@this.FullName + " does not have a property named " + name);
        }

        public static IEnumerable<MemberInfo> GetPropertiesAndFields(this Type @this, BindingFlags flags) =>
            @this.GetProperties(flags).Cast<MemberInfo>().Concat(@this.GetFields(flags));

        public static Type GetPropertyOrFieldType(this MemberInfo @this) =>
            (@this as PropertyInfo)?.PropertyType ?? (@this as FieldInfo)?.FieldType;

        static IEnumerable<Assembly> GetReferencingAssemblies(Assembly anotherAssembly) =>
            AppDomain.CurrentDomain.GetAssemblies().Where(assebly => assebly.References(anotherAssembly));

        public static IEnumerable<Type> SelectTypesByAttribute<T>(this Assembly @this, bool inherit) where T : Attribute =>
            @this.GetExportedTypes().Where(t => t.IsDefined(typeof(T), inherit));

        /// <summary>
        /// Gets all types in the current appDomain which implement this interface.
        /// </summary>
        public static List<Type> FindImplementerClasses(this Type @this)
        {
            if (!@this.IsInterface) throw new InvalidOperationException(@this.GetType().FullName + " is not an Interface.");

            var result = new List<Type>();

            foreach (var assembly in GetReferencingAssemblies(@this.Assembly))
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type == @this) continue;
                        if (!type.IsClass) continue;

                        if (type.Implements(@this)) result.Add(type);
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

        public static object GetObjectByPropertyPath(this Type @this, object instance, string propertyPath)
        {
            if (propertyPath.Contains("."))
            {
                var directProperty = @this.GetProperty(propertyPath.RemoveFrom("."));

                if (directProperty == null)
                    throw new Exception(@this.FullName + " does not have a property named '" + propertyPath.RemoveFrom(".") + "'");

                var associatedObject = directProperty.GetValue(instance);
                if (associatedObject == null) return null;

                var remainingPath = propertyPath.TrimStart(directProperty.Name + ".");
                return associatedObject.GetType().GetObjectByPropertyPath(associatedObject, remainingPath);
            }
            else
            {
                return @this.GetProperty(propertyPath).GetValue(instance);
            }
        }

        /// <summary>
        /// Creates a new thread and copies the current Culture and UI Culture.
        /// </summary>
        public static Thread CreateNew(this Thread @this, Action threadStart) => CreateNew(@this, threadStart, null);

        /// <summary>
        /// Creates a new thread and copies the current Culture and UI Culture.
        /// </summary>
        public static Thread CreateNew(this Thread @this, Action threadStart, Action<Thread> initializer)
        {
            var result = new Thread(new ThreadStart(threadStart));

            initializer?.Invoke(result);

            return result;
        }

        /// <summary>
        /// Gets the default value for this type. It's equivalent to default(T).
        /// </summary>
        public static object GetDefaultValue(this Type @this)
        {
            if (@this.IsValueType)
                return Activator.CreateInstance(@this);

            return null;
        }

        /// <summary>
        /// If it specifies DisplayNameAttribute the value from that will be returned.
        /// Otherwise it returns natural English literal text for the name of this member.
        /// For example it coverts "ThisIsSomething" to "This is something".
        /// </summary>
        public static string GetDisplayName(this MemberInfo @this)
        {
            var byAttribute = @this.GetCustomAttribute<System.ComponentModel.DisplayNameAttribute>()?.DisplayName;
            return byAttribute.Or(() => @this.Name.ToLiteralFromPascalCase());
        }

        /// <summary>
        /// Determine whether this property is static.
        /// </summary>
        public static bool IsStatic(this PropertyInfo @this) => (@this.GetGetMethod() ?? @this.GetSetMethod()).IsStatic;

        public static TTArget GetTargetOrDefault<TTArget>(this WeakReference<TTArget> @this)
            where TTArget : class
        {
            if (@this == null) return null;

            if (@this.TryGetTarget(out var result)) return result;
            return null;
        }

        internal static WeakReference<T> GetWeakReference<T>(this T item) where T : class
            => new WeakReference<T>(item);

        public static object GetValue(this MemberInfo @this, object obj)
        {
            if (@this is PropertyInfo asProp) return asProp.GetValue(obj);

            if (@this is FieldInfo asField) return asField.GetValue(obj);

            if (@this is MethodInfo asMethod) return asMethod.Invoke(obj, new object[0]);

            throw new Exception("GetValue() is not implemented for " + @this?.GetType().Name);
        }

        public static bool IsIEnumerableOf(this Type @this, Type typeofT)
        {
            return @this.IsA(typeof(IEnumerable<>).MakeGenericType(typeofT));
        }

        /// <summary>
        /// Returns an instnce public property with the specified name.
        /// It avoids AmbiguousMatchFoundException by searching the types one by one.
        /// </summary> 
        public static PropertyInfo SafeGetProperty(this Type @this, string propertyName)
        {
            if (@this == null) throw new ArgumentNullException(nameof(@this));
            if (propertyName.IsEmpty()) throw new ArgumentNullException(nameof(propertyName));

            var result = @this.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            if (result != null) return result;

            return @this.BaseType.SafeGetProperty(propertyName);
        }

        /// <summary>
        /// If this type implements IEnumerable«T» it returns typeof(T).
        /// </summary>
        public static Type GetEnumerableItemType(this Type @this)
        {
            if (!@this.Implements<IEnumerable>()) return null;

            if (@this.IsArray) return @this.GetElementType();

            if (@this.IsGenericType)
            {
                var implementedIEnumerableT = @this.GetInterfaces().FirstOrDefault(x =>
                x.GetGenericArguments().IsSingle() &&
                x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                return implementedIEnumerableT?.GetGenericArguments().Single();
            }

            return null;
        }

        public static bool IsBasicNumeric(this Type @this)
        {
            if (@this == typeof(int)) return true;
            if (@this == typeof(float)) return true;
            if (@this == typeof(double)) return true;
            if (@this == typeof(short)) return true;
            if (@this == typeof(decimal)) return true;
            if (@this == typeof(long)) return true;

            return false;
        }
    }
}