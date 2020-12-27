using System;
using dnlib.DotNet;
using dnSpy.Contracts.Settings;

namespace HoLLy.dnSpyExtension.Common.CodeInjection
{
    internal struct InjectionArguments
    {
        public string Path;
        public string? Namespace;
        public string Type;
        public string Method;
        public string? Argument;

        public string TypeFull => Namespace is null ? Type : Namespace + "." + Type;

        public static InjectionArguments FromMethodDef(MethodDef method, string? parameter) => new() {
            Path = method.Module.Location, Namespace = method.DeclaringType.Namespace, Type = method.DeclaringType.Name, Method = method.Name, Argument = parameter
        };

        public static InjectionArguments FromSection(ISettingsSection section) => new() {
            Path = section.Attribute<string?>(nameof(Path)) ?? throw new Exception($"Couldn't find {nameof(Path)} attribute in settings section"),
            Namespace = section.Attribute<string?>(nameof(Namespace)),
            Type = section.Attribute<string?>(nameof(Type)) ?? throw new Exception($"Couldn't find {nameof(Type)} attribute in settings section"),
            Method = section.Attribute<string?>(nameof(Method)) ?? throw new Exception($"Couldn't find {nameof(Method)} attribute in settings section"),
            Argument = section.Attribute<string?>(nameof(Argument)),
        };
    }
}
