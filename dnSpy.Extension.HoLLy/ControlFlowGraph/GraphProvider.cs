using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.Core.Graphing;
using Echo.Platforms.Dnlib;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using Node = Microsoft.Msagl.Drawing.Node;

namespace HoLLy.dnSpyExtension.ControlFlowGraph
{
    public abstract class GraphProvider
    {
        public abstract Graph ToMicrosoftGraph();

        public static GraphProvider Create(MethodDef method)
        {
            var arch = new CilArchitecture(method);
            var stateResolver = new CilStateTransitionResolver(arch);
            var cflowBuilder = new SymbolicFlowGraphBuilder<Instruction>(arch, method.Body.Instructions, stateResolver);
            var cflow = cflowBuilder.ConstructFlowGraph(0);

            return new ControlFlowGraphProvider<Instruction>(cflow);
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
                var newGraph = new Graph();
                foreach (var node in graph.Nodes)
                {
                    newGraph.AddNode(new Node(getId(node))
                    {
                        LabelText = node.Contents.ToString(),
                    });
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

                newGraph.LayoutAlgorithmSettings.PackingMethod = PackingMethod.Columns;

                return newGraph;

                static string getId(INode node) => node.Id.ToString("X16");
            }
        }
    }
}