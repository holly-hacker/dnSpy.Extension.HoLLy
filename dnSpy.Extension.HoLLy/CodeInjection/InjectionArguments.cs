using dnlib.DotNet;

namespace HoLLy.dnSpyExtension.CodeInjection
{
    public struct InjectionArguments
    {
        public string Path;
        public string Type;
        public string Method;
        public string Argument;

        public static InjectionArguments FromMethodDef(MethodDef method, string parameter) => new InjectionArguments {
            Path = method.Module.Location, Type = method.DeclaringType.FullName, Method = method.Name, Argument = parameter
        };
    }
}
