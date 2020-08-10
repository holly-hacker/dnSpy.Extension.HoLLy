using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Settings.Fonts;
using dnSpy.Contracts.Themes;

namespace HoLLy.dnSpyExtension.ControlFlowGraph
{
    public class ControlFlowGraphTabContent : DocumentTabContent
    {
        private readonly GraphProvider graphProvider;
        private readonly ITheme theme;
        private readonly FontSettings font;

        public override string Title => $"CFG: {graphProvider.MethodName}";

        public ControlFlowGraphTabContent(GraphProvider graphProvider, ITheme theme, FontSettings font)
        {
            this.graphProvider = graphProvider;
            this.theme = theme;
            this.font = font;
        }

        public override DocumentTabContent Clone()
        {
            return new ControlFlowGraphTabContent(graphProvider, theme, font);
        }

        public override DocumentTabUIContext CreateUIContext(IDocumentTabUIContextLocator locator)
        {
            return new ControlFlowGraphTabUiContext(graphProvider, theme, font);
        }
    }
}