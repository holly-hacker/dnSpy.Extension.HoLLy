using System;
using HoLLy.dnSpyExtension.Common.CodeInjection;

namespace HoLLy.dnSpyExtension.CodeInjection.Injectors
{
    public interface IInjector
    {
        Action<string> Log { set; }
        void Inject(int pid, in InjectionArguments args, bool x86);
    }
}
