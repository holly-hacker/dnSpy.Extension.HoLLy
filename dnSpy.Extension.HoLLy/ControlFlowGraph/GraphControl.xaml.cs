using System.Windows.Controls;
using dnSpy.Contracts.Themes;
using Microsoft.Msagl.WpfGraphControl;

namespace HoLLy.dnSpyExtension.ControlFlowGraph
{
    public partial class GraphControl : UserControl
    {
        public GraphControl(GraphProvider echoGraph, ITheme theme)
        {
            InitializeComponent();

            var viewer = new GraphViewer();
            viewer.BindToPanel(Panel);
            viewer.Graph = echoGraph.ToMicrosoftGraph(theme);
        }
    }
}