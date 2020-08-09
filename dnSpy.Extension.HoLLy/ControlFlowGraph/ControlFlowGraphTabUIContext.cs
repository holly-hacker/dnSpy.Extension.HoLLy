using System.Windows;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Themes;

namespace HoLLy.dnSpyExtension.ControlFlowGraph
{
    public class ControlFlowGraphTabUiContext : DocumentTabUIContext
    {
        readonly GraphControl content;

        public ControlFlowGraphTabUiContext(GraphProvider graph, ITheme theme)
        {
            content = new GraphControl(graph, theme);
        }

        public override object? UIObject => content;
        public override IInputElement? FocusedElement => content;
        public override FrameworkElement? ZoomElement => content;
    }
}