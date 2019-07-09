using dnSpy.Contracts.Debugger;

namespace HoLLy.dnSpyExtension.Common.CodeInjection
{
    public interface IManagedInjector
    {
        void Inject(int pid, InjectionArguments args, bool x86, RuntimeType runtimeType);
        void Inject(int pid, string path, string typeName, string methodName, string? parameter, bool x86, RuntimeType runtimeType);
        bool IsProcessSupported(DbgProcess process, out string? reason);
    }
}
