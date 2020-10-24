using System.ComponentModel.Composition;
using dnSpy.Contracts.Extension;
using dnSpy.Contracts.Output;
using dnSpy.Contracts.Text;

namespace HoLLy.dnSpyExtension.Common.Logging
{
    public static class ExtensionLogger
    {
        public static ITextColorWriter Instance => CachedInstance;
        private static readonly CachedTextColorWriter CachedInstance = new CachedTextColorWriter();

        [ExportAutoLoaded(LoadType = AutoLoadedLoadType.AppLoaded, Order = double.MinValue)]
        private sealed class InitializeLogger : IAutoLoaded
        {
            [ImportingConstructor]
            private InitializeLogger(IOutputService outputService)
            {
                var pane = outputService.Create(Constants.LoggerOutputPane, "dnSpy.Extension.HoLLy");
                CachedInstance.Writer = pane;
            }
        }
    }
}