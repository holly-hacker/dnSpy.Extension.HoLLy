using System;
using System.Linq;
using System.Text;
using System.Windows.Media;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Settings.Fonts;
using dnSpy.Contracts.Themes;
using Echo.ControlFlow;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.Core.Graphing;
using Echo.Platforms.Dnlib;
using HoLLy.dnSpyExtension.NativeDisassembler;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Drawing;
using Color = Microsoft.Msagl.Drawing.Color;
using Node = Microsoft.Msagl.Drawing.Node;

namespace HoLLy.dnSpyExtension.ControlFlowGraph
{
    public abstract class GraphProvider
    {
        public abstract string MethodName { get; }
        public abstract Graph ToMicrosoftGraph(ITheme theme, FontSettings font);

        public static GraphProvider Create(MethodDef method)
        {
            var methodName = IdentifierEscaper.Escape(method.Name);
            switch (method.MethodBody)
            {
                case CilBody cilBody:
                {
                    var arch = new CilArchitecture(method);
                    var stateResolver = new CilStateTransitionResolver(arch);
                    var cflowBuilder = new SymbolicFlowGraphBuilder<Instruction>(arch, method.Body.Instructions, stateResolver);
                    var handlers = cilBody.ExceptionHandlers.Select(DnlibExtensions.ToEchoRange);
                    var cflow = cflowBuilder.ConstructFlowGraph(0, handlers);
                    return new ControlFlowGraphProvider<Instruction>(methodName, cflow);
                }
                case NativeMethodBody _:
                {
                    var cflow = IcedHelpers.ReadNativeMethodBody(method);
                    return new ControlFlowGraphProvider<Iced.Intel.Instruction>(methodName, cflow);
                }
                default:
                    throw new Exception("Tried to create graph for method that has neither managed nor native body");
            }
        }

        private static Graph CreateEmptyGraph()
        {
            return new Graph
            {
                LayoutAlgorithmSettings =
                {
                    PackingMethod = PackingMethod.Columns,

                    EdgeRoutingSettings = new EdgeRoutingSettings
                    {
                        EdgeRoutingMode = EdgeRoutingMode.Rectilinear,
                        CornerRadius = 5.0,
                    }
                },
                Attr =
                {
                    BackgroundColor = Color.Transparent,
                    LayerDirection = LayerDirection.TB, // prefer top-to-bottom layout
                },
            };
        }

        private class ControlFlowGraphProvider<TInstruction> : GraphProvider
        {
            private readonly ControlFlowGraph<TInstruction> graph;

            public ControlFlowGraphProvider(string methodName, ControlFlowGraph<TInstruction> graph)
            {
                MethodName = methodName;
                this.graph = graph;
            }
            
            public override string MethodName { get; }
            
            public override Graph ToMicrosoftGraph(ITheme theme, FontSettings font)
            {
                var newGraph = CreateEmptyGraph();

                Color textColor = theme.GetColor(ColorType.Text).Foreground is SolidColorBrush sb
                    ? BrushToColor(sb)
                    : theme.IsDark
                        ? Color.LightGray
                        : Color.DarkGray;
                Color fillColor = theme.GetColor(ColorType.Text).Background is SolidColorBrush sb2
                    ? BrushToColor(sb2)
                    : theme.IsDark
                        ? Color.Black
                        : Color.White;
                Color green = theme.GetColor(ColorType.Green).Foreground is SolidColorBrush sb3
                    ? BrushToColor(sb3)
                    : Color.Green;
                Color red = theme.GetColor(ColorType.Red).Foreground is SolidColorBrush sb4
                    ? BrushToColor(sb4)
                    : Color.Red;
                Color gray = theme.GetColor(ColorType.Gray).Foreground is SolidColorBrush sb5
                    ? BrushToColor(sb5)
                    : Color.Gray;

                foreach (var node in graph.Nodes)
                {
                    var newNode = new Node(getId(node))
                    {
                        LabelText = GetBlockText(node.Contents),
                        Attr =
                        {
                            FillColor = fillColor,
                            LabelMargin = 4,
                        },
                        Label =
                        {
                            FontColor = textColor,
                            FontName = font.FontFamily.ToString(),
                            FontSize = font.FontSize,
                        },
                        UserData = node.Id,
                    };
                    newGraph.AddNode(newNode);
                }
                
                foreach (var edge in graph.GetEdges())
                {
                    var newEdge = newGraph.AddEdge(getId(edge.Origin), getId(edge.Target));
                    newEdge.Attr.Color = edge.Type switch
                    {
                        ControlFlowEdgeType.Abnormal => gray,
                        ControlFlowEdgeType.FallThrough when edge.Origin.ConditionalEdges.Count > 0 => red,
                        ControlFlowEdgeType.Conditional => green,
                        ControlFlowEdgeType.FallThrough => textColor,
                        ControlFlowEdgeType.None => throw new InvalidOperationException("Found a None node in control flow graph, Echo bug?"),
                        _ => throw new IndexOutOfRangeException("Unknown edge type: " + edge.Type),
                    };
                }

                return newGraph;

                static string getId(IIdentifiedNode node) => node.Id.ToString("X16");
            }

            private static string GetBlockText(BasicBlock<TInstruction> nodeContents)
            {
                var sb = new StringBuilder();

                switch (nodeContents)
                {
                    case BasicBlock<Instruction> cilBlock:
                    {
                        sb.AppendLine($"IL_{cilBlock.Header.Offset:X8}:\n");

                        foreach (var cilInstruction in cilBlock.Instructions)
                        {
                            sb.Append(cilInstruction.OpCode.Name);
                            InstructionPrinter.AddOperandString(sb, cilInstruction, " ");
                            sb.AppendLine();
                        }

                        break;
                    }
                    default:
                        sb.Append(nodeContents);
                        break;
                }

                return sb.ToString().TrimEnd();
            }
        }

        private static Color BrushToColor(SolidColorBrush brush)
        {
            var c = brush.Color;
            // MsgBox.Instance.Show($"Color: {c.R}, {c.G}, {c.B}, {c.A}");
            return new Color(c.A, c.R, c.G, c.B);
        }
    }
}