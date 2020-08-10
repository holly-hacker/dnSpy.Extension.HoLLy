using System.Linq;
using System.Windows;
using System.Windows.Controls;
using dnSpy.Contracts.Settings.Fonts;
using dnSpy.Contracts.Themes;
using Microsoft.Msagl.WpfGraphControl;

namespace HoLLy.dnSpyExtension.ControlFlowGraph
{
    public partial class GraphControl : UserControl
    {
        public GraphControl(GraphProvider echoGraph, ITheme theme, FontSettings font)
        {
            InitializeComponent();

            // could set LastChildFill to true, doesn't seem to matter
            Panel.HorizontalAlignment = HorizontalAlignment.Center;
            Panel.VerticalAlignment = VerticalAlignment.Center;

            var viewer = new GraphViewer();
            viewer.BindToPanel(Panel);
            viewer.Graph = echoGraph.ToMicrosoftGraph(theme, font);

            viewer.NodeToCenterWithScale(viewer.Graph.Nodes.OrderBy(n => n.UserData is long u ? u : long.MaxValue).First(), 1);
        }
    }
}