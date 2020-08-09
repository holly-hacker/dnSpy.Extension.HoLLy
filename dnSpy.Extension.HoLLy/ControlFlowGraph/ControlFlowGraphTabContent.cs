using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Themes;

namespace HoLLy.dnSpyExtension.ControlFlowGraph
{
    public class ControlFlowGraphTabContent : DocumentTabContent
    {
        private readonly GraphProvider graphProvider;
        private readonly ITheme theme;

        public override string Title => "CFG";

        public ControlFlowGraphTabContent(GraphProvider graphProvider, ITheme theme)
        {
            this.graphProvider = graphProvider;
            this.theme = theme;
        }

        public override DocumentTabContent Clone()
        {
            return new ControlFlowGraphTabContent(graphProvider, theme);
        }

        public override DocumentTabUIContext CreateUIContext(IDocumentTabUIContextLocator locator)
        {
            return new ControlFlowGraphTabUiContext(graphProvider, theme);
        }
    }
}