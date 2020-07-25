using System.Windows;
using dnSpy.Contracts.Documents.Tabs;

namespace HoLLy.dnSpyExtension.ControlFlowGraph
{
    public class ControlFlowGraphTabUIContext : DocumentTabUIContext
    {
        readonly GraphControl content;

        public ControlFlowGraphTabUIContext(GraphProvider graph)
        {
            content = new GraphControl(graph);
        }

        public override object? UIObject => content;
        public override IInputElement? FocusedElement => content;
        public override FrameworkElement? ZoomElement => content;
    }
}