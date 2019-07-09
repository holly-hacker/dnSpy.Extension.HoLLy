using System;
using dnlib.DotNet;
using dnSpy.Contracts.Settings;

namespace HoLLy.dnSpyExtension.Common.CodeInjection
{
    public struct InjectionArguments
    {
        public string Path;
        public string Type;
        public string Method;
        public string? Argument;

        public static InjectionArguments FromMethodDef(MethodDef method, string? parameter) => new InjectionArguments {
            Path = method.Module.Location, Type = method.DeclaringType.FullName, Method = method.Name, Argument = parameter
        };

        public static InjectionArguments FromSection(ISettingsSection section) => new InjectionArguments {
            Path = section.Attribute<string?>(nameof(Path)) ?? throw new Exception($"Couldn't find {nameof(Path)} attribute in settings section"),
            Type = section.Attribute<string?>(nameof(Type)) ?? throw new Exception($"Couldn't find {nameof(Type)} attribute in settings section"),
            Method = section.Attribute<string?>(nameof(Method)) ?? throw new Exception($"Couldn't find {nameof(Method)} attribute in settings section"),
            Argument = section.Attribute<string?>(nameof(Argument)),
        };
    }
}
