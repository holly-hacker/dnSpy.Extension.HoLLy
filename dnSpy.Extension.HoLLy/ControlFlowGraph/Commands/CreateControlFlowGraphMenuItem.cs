using System.ComponentModel.Composition;
using dnlib.DotNet;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.Settings.Fonts;
using dnSpy.Contracts.Themes;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.Commands;

namespace HoLLy.dnSpyExtension.ControlFlowGraph.Commands
{
    [ExportMenuItem(Header = "Create CFG", Group = Constants.ContextMenuGroupEdit, Order = 10000)]
    public class CreateControlFlowGraphMenuItem : MethodMenuItemBase
    {
        private readonly IDocumentTabService documentTabService;
        private readonly IThemeService themeService;
        private readonly ThemeFontSettingsService fontService;

        [ImportingConstructor]
        public CreateControlFlowGraphMenuItem(IDocumentTabService documentTabService, IThemeService themeService, ThemeFontSettingsService fontService)
        {
            this.documentTabService = documentTabService;
            this.themeService = themeService;
            this.fontService = fontService;
        }

        protected override void Execute(MethodDef method)
        {
            var graphProvider = GraphProvider.Create(method);

            // TODO: automatically update theme?
            var font = fontService.GetSettings("text").Active;
            var newTab = documentTabService.OpenEmptyTab();
            newTab.Show(new ControlFlowGraphTabContent(graphProvider, themeService.Theme, font), null, null);
        }
    }
}