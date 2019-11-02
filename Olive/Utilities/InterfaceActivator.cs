using System;
using System.Linq;
using System.Collections.Concurrent;

namespace Olive
{
    public static class InterfaceActivator
    {
        static readonly ConcurrentDictionary<Type, Type> Mapping = new ConcurrentDictionary<Type, Type>();

        public static object CreateInstance(Type interfaceType, params object[] args)
        {
            if (Mapping.TryGetValue(interfaceType, out var classType)) 
                return CreateInstanceImpl(classType, args);

            var implmenters = interfaceType.FindImplementerClasses();

            if (implmenters.None())
                throw new Exception($"No type in the currently loaded assemblies implements {interfaceType.FullName}.");

            if (implmenters.HasMany())
                throw new Exception($"More than one type in the currently loaded assemblies implement {interfaceType.FullName}: {implmenters.Select(x => x.FullName).ToString(" and ")}");

            var type = implmenters.First();

            Mapping.TryAdd(interfaceType, type);

            return CreateInstanceImpl(type, args);
        }

        public static T CreateInstance<T>(params object[] args) where T : class => 
            CreateInstance(typeof(T), args) as T;

        static object CreateInstanceImpl(Type type, params object[] args)
        {
            if (args.None())
                return Activator.CreateInstance(type);
            else
                return Activator.CreateInstance(type, args);
        }
    }
}
