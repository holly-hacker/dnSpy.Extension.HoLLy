using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
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
            Panel.HorizontalAlignment = HorizontalAlignment.Stretch;
            Panel.VerticalAlignment = VerticalAlignment.Stretch;

            var viewer = new GraphViewer();
            viewer.BindToPanel(Panel);
            viewer.Graph = echoGraph.ToMicrosoftGraph(theme, font);
            SetTooltips(viewer);
            viewer.NodeToCenterWithScale(viewer.Graph.Nodes.OrderBy(n => n.UserData is long u ? u : long.MaxValue).First(), 1);
        }

        private static void SetTooltips(GraphViewer viewer)
        {
            try
            {
                // this is very ugly, but I don't know a better way
                const string labelFieldName = "FrameworkElementOfNodeForLabel";
                const string pathFieldName = "BoundaryPath";
                var labelField = typeof(VNode).GetField(labelFieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                                 ?? throw new Exception($"Couldn't find {nameof(VNode)} field {labelFieldName}");
                var pathField = typeof(VNode).GetField(pathFieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                                ?? throw new Exception($"Couldn't find {nameof(VNode)} field {pathFieldName}");

                foreach (var viewerEntity in viewer.Entities)
                {
                    if (!(viewerEntity is VNode vNode)) continue;

                    (string? labelTooltip, string? pathTooltip) = GetTooltipsForNode(vNode);

                    var label = (FrameworkElement?) labelField.GetValue(vNode)
                                ?? throw new Exception($"Found {nameof(vNode)} with null {nameof(labelFieldName)}");
                    label.ToolTip = labelTooltip;

                    var path = (Path?) pathField.GetValue(vNode)
                               ?? throw new Exception($"Found {nameof(vNode)} with null {nameof(pathFieldName)}");
                    path.ToolTip = pathTooltip;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to set graph control tooltips: " + e.Message);
            }
        }

        private static (string? labelTooltip, string? pathTooltip) GetTooltipsForNode(VNode _)
        {
            // not sure yet what to put as tooltip
            return (null, null);
        }
    }
}