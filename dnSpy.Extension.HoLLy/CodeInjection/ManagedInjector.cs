using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using dnSpy.Contracts.Debugger;
using HoLLy.dnSpyExtension.CodeInjection.Injectors;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.CodeInjection;
using Iced.Intel;

namespace HoLLy.dnSpyExtension.CodeInjection
{
    [Export(typeof(IManagedInjector))]
    internal class ManagedInjector : IManagedInjector
    {
        private DbgManager DbgManager => dbgManagerLazy.Value;
        private readonly Lazy<DbgManager> dbgManagerLazy;
        private readonly Settings settings;

        [ImportingConstructor]
        public ManagedInjector(Lazy<DbgManager> dbgManagerLazy, Settings settings)
        {
            this.dbgManagerLazy = dbgManagerLazy;
            this.settings = settings;
        }

        public void Inject(int pid, InjectionArguments args, bool x86, RuntimeType runtimeType)
            => Inject(pid, args.Path, args.Type, args.Method, args.Argument, x86, runtimeType);

        public void Inject(int pid, string path, string typeName, string methodName, string? parameter, bool x86, RuntimeType runtimeType)
        {
            if (settings.CopyInjectedDLLToTemp)
                path = Utils.CopyToTempPath(path);

            var injector = GetInjector(runtimeType);
            injector.Inject(pid, path, typeName, methodName, parameter, x86);
        }

        private static IInjector GetInjector(RuntimeType runtimeType)
        {
            switch (runtimeType) {
                case RuntimeType.FrameworkV2: return new FrameworkV2Injector();
                case RuntimeType.FrameworkV4: return new FrameworkV4Injector();
                case RuntimeType.NetCore:
                case RuntimeType.Unity:
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
                _ => false,
            };

            if (!rtSupported) {
                reason = $"Unsupported runtime '{process.Runtimes.First().Name}'";
                return false;
            }

            reason = null;
            return true;
        }
    }
}
