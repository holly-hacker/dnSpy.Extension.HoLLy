using dnSpy.Contracts.Documents.Tabs;

namespace HoLLy.dnSpyExtension.ControlFlowGraph
{
    public class ControlFlowGraphTabContent : DocumentTabContent
    {
        private readonly GraphProvider graphProvider;

        public override string Title => "CFG";

        public ControlFlowGraphTabContent(GraphProvider graphProvider)
        {
            this.graphProvider = graphProvider;
        }

        public override DocumentTabContent Clone()
        {
            return new ControlFlowGraphTabContent(graphProvider);
        }

        public override DocumentTabUIContext CreateUIContext(IDocumentTabUIContextLocator locator)
        {
            return new ControlFlowGraphTabUIContext(graphProvider);
        }
    }
}