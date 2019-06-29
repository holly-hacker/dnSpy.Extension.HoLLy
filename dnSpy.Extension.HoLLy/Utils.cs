using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace HoLLy.dnSpy.Extension
{
    internal static class Utils
    {
        public static bool IsDebugBuild => IsDebugBuildLazy.Value;

        private static readonly Lazy<bool> IsDebugBuildLazy = new Lazy<bool>(() => IsAssemblyDebugBuild(typeof(Utils).Assembly));

        /// <remarks>https://stackoverflow.com/a/2186634</remarks>
        private static bool IsAssemblyDebugBuild(Assembly assembly)
        {
            return assembly.GetCustomAttributes(false).OfType<DebuggableAttribute>().Any(da => da.IsJITTrackingEnabled);
        }
    }
}
