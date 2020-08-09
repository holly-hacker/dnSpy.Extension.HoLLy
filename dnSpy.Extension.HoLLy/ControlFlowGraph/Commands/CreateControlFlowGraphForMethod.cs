using System.ComponentModel.Composition;
using dnlib.DotNet;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.Themes;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension.ControlFlowGraph.Commands
{
    [ExportMenuItem(Header = "Create CFG", Group = Constants.ContextMenuGroupEdit, Order = 10000)]
    public class CreateControlFlowGraphForMethod : MenuItemBase<MethodDef>
    {
        private readonly IDocumentTabService documentTabService;
        private readonly IThemeService themeService;
        protected override object CachedContextKey => new object();

        [ImportingConstructor]
        public CreateControlFlowGraphForMethod(IDocumentTabService documentTabService, IThemeService themeService)
        {
            this.documentTabService = documentTabService;
            this.themeService = themeService;
        }

        protected override MethodDef? CreateContext(IMenuItemContext context)
        {
            if (context.Find<TextReference>()?.Reference is MethodDef md && md.MethodBody != null)
                return md;

            return null;
        }

        public override void Execute(MethodDef context)
        {
            var graphProvider = GraphProvider.Create(context);

            // TODO: automatically update theme?
            var newTab = documentTabService.OpenEmptyTab();
            newTab.Show(new ControlFlowGraphTabContent(graphProvider, themeService.Theme), null, null);
        }
    }
}