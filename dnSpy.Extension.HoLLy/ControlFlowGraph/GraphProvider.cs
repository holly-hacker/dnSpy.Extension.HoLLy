using System;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.Core.Graphing;
using Echo.Platforms.Dnlib;
using HoLLy.dnSpyExtension.NativeDisassembler;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Drawing;
using Node = Microsoft.Msagl.Drawing.Node;

namespace HoLLy.dnSpyExtension.ControlFlowGraph
{
    public abstract class GraphProvider
    {
        public abstract Graph ToMicrosoftGraph();

        public static GraphProvider Create(MethodDef method)
        {
            switch (method.MethodBody)
            {
                case CilBody _:
                {
                    var arch = new CilArchitecture(method);
                    var stateResolver = new CilStateTransitionResolver(arch);
                    var cflowBuilder = new SymbolicFlowGraphBuilder<Instruction>(arch, method.Body.Instructions, stateResolver);
                    var cflow = cflowBuilder.ConstructFlowGraph(0);
                    return new ControlFlowGraphProvider<Instruction>(cflow);
                }
                case NativeMethodBody _:
                {
                    var cflow = IcedHelpers.ReadNativeMethodBody(method);
                    return new ControlFlowGraphProvider<Iced.Intel.Instruction>(cflow);
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

            public ControlFlowGraphProvider(ControlFlowGraph<TInstruction> graph)
            {
                this.graph = graph;
            }
            
            public override Graph ToMicrosoftGraph()
            {
                var newGraph = CreateEmptyGraph();

                foreach (var node in graph.Nodes)
                {
                    newGraph.AddNode(new Node(getId(node)) {LabelText = node.Contents.ToString()});
                }
                
                foreach (var edge in graph.GetEdges())
                {
                    var newEdge = newGraph.AddEdge(getId(edge.Origin), getId(edge.Target));
                    newEdge.Attr.Color = edge.Type switch
                    {
                        ControlFlowEdgeType.Abnormal => Color.Gray,
                        ControlFlowEdgeType.FallThrough when edge.Origin.ConditionalEdges.Count > 0 => Color.Red,
                        ControlFlowEdgeType.Conditional => Color.Green,
                        ControlFlowEdgeType.FallThrough => Color.Black,
                        _ => Color.Black,
                    };
                }

                return newGraph;

                static string getId(INode node) => node.Id.ToString("X16");
            }
        }
    }
}