using System;
using System.ComponentModel.Composition;
using System.Linq;
using dnSpy.Contracts.Debugger;
using HoLLy.dnSpyExtension.CodeInjection.Injectors;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.CodeInjection;

namespace HoLLy.dnSpyExtension.CodeInjection
{
    [Export(typeof(IManagedInjector))]
    internal class ManagedInjector : IManagedInjector
    {
        public Action<string> Log { private get; set; } = s => { };

        private readonly Settings settings;

        [ImportingConstructor]
        public ManagedInjector(Settings settings)
        {
            this.settings = settings;
        }

        public void Inject(int pid, in InjectionArguments args, bool x86, RuntimeType runtimeType)
        {
            var injector = GetInjector(runtimeType);
            injector.Log = Log;

            if (!settings.CopyInjectedDllToTemp) {
                injector.Inject(pid, args, x86);
            } else {
                injector.Inject(pid, new InjectionArguments {
                    Path = Utils.CopyToTempPath(args.Path),
                    Namespace = args.Namespace,
                    Type = args.Type,
                    Method = args.Method,
                    Argument = args.Argument,
                }, x86);
            }

        }

        private static IInjector GetInjector(RuntimeType runtimeType)
        {
            switch (runtimeType) {
                case RuntimeType.FrameworkV2: return new FrameworkV2Injector();
                case RuntimeType.FrameworkV4: return new FrameworkV4Injector();
                case RuntimeType.Unity: return new UnityInjector();
                case RuntimeType.NetCore:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(runtimeType), runtimeType, null);
            }
        }

        public bool IsProcessSupported(DbgProcess process, out string? reason)
        {
            if (process is null) {
                reason = "no process found";
                return false;
            }

            if (process.OperatingSystem != DbgOperatingSystem.Windows) {
                reason = "Windows only";
                return false;
            }

            if (process.Architecture != DbgArchitecture.X86 && process.Architecture != DbgArchitecture.X64) {
                reason = "x86 and x64 only";
                return false;
            }

            var runtimeType = process.Runtimes.First().GetRuntimeType();
            var rtSupported = runtimeType switch {
                RuntimeType.FrameworkV2 => true,
                RuntimeType.FrameworkV4 => true,
                RuntimeType.Unity when process.Architecture == DbgArchitecture.X86 => true,
                _ => false,
            };

            if (!rtSupported) {
                reason = $"Unsupported runtime '{process.Runtimes.First().Name}' on architecture {process.Architecture}";
                return false;
            }

            reason = null;
            return true;
        }
    }
}
