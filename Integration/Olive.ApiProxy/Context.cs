using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Olive.ApiProxy
{
    class Context
    {
        public static string PublisherService, ControllerName;
        public static FileInfo AssemblyFile;
        public static DirectoryInfo Output;
        public static Assembly Assembly;
        public static Type ControllerType;
        public static MethodGenerator[] ActionMethods;
    }
}
