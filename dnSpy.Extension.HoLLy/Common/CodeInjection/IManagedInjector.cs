using dnSpy.Contracts.Debugger;

namespace HoLLy.dnSpyExtension.Common.CodeInjection
{
    public interface IManagedInjector
    {
        void Inject(int pid, in InjectionArguments args, bool x86, RuntimeType runtimeType);
        bool IsProcessSupported(DbgProcess process, out string? reason);
    }
}
