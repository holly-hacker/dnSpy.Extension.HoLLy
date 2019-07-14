namespace HoLLy.dnSpyExtension.CodeInjection.Injectors
{
    public interface IInjector
    {
        void Inject(int pid, string path, string typeName, string methodName, string? parameter, bool x86);
    }
}
