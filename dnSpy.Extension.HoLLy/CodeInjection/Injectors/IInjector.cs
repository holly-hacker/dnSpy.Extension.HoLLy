using System;

namespace HoLLy.dnSpyExtension.CodeInjection.Injectors
{
    public interface IInjector
    {
        Action<string> Log { set; }
        void Inject(int pid, string path, string typeName, string methodName, string? parameter, bool x86);
    }
}
