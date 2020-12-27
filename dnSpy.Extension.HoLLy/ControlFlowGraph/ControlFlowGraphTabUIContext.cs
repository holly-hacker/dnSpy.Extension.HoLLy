using System.Windows;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Settings.Fonts;
using dnSpy.Contracts.Themes;

namespace HoLLy.dnSpyExtension.ControlFlowGraph
{
    public class ControlFlowGraphTabUiContext : DocumentTabUIContext
    {
        private readonly GraphControl content;

        public ControlFlowGraphTabUiContext(GraphProvider graph, ITheme theme, FontSettings font)
        {
            content = new GraphControl(graph, theme, font);
        }

        public override object UIObject => content;
        public override IInputElement FocusedElement => content;
        public override FrameworkElement ZoomElement => content;
    }
}