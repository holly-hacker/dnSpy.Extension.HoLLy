using dnlib.DotNet;
using dnSpy.Contracts.Settings;

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

        public static InjectionArguments FromSection(ISettingsSection section) => new InjectionArguments {
                Path = section.Attribute<string?>(nameof(Path)),
                Type = section.Attribute<string?>(nameof(Type)),
                Method = section.Attribute<string?>(nameof(Method)),
                Argument = section.Attribute<string?>(nameof(Argument)),
            };
    }
}
