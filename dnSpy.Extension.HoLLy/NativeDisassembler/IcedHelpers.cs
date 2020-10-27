using System;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.PE;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.Platforms.Iced;
using Iced.Intel;

namespace HoLLy.dnSpyExtension.NativeDisassembler
{
    public static class IcedHelpers
    {
        public static byte[] ReadNativeMethodBodyBytes(MethodDef method)
            => EncodeBytes(GetInstructionsFromGraph(ReadNativeMethodBody(method)), !method.Module.IsAMD64);
        
        public static ControlFlowGraph<Instruction> ReadNativeMethodBody(MethodDef method)
        {
            var mod = method.Module;
            if (!(mod is ModuleDefMD md))
                throw new NotSupportedException($"Cannot read native function of a {mod.GetType()}");

            bool is32Bit = !method.Module.IsAMD64;
            var rva = method.NativeBody.RVA;
            var image = md.Metadata.PEImage;

            return ReadNativeFunction(image, rva, is32Bit);
        }

        public static ControlFlowGraph<Instruction> ReadNativeFunction(IPEImage image, RVA rva, bool is32Bit)
        {
            var reader = image.CreateReader(rva);
            var stream = reader.AsStream(); // does not need to be disposed by the caller

            var architecture = new X86Architecture();
            // NOTE: the InstructionProvider will seek based on the value passed to ConstructFlowGraph. Therefore, we
            // pass it as the base address in the ctor, meaning the first instruction will be at index
            // (entrypoint - BaseAddress) == 0 in the stream derived from the reader.
            // Position 0 of the stream is position 0 of the reader, which is the file offset calculated from the rva.
            var instructionProvider = new X86DecoderInstructionProvider(architecture, stream, is32Bit ? 32 : 64, (uint)rva, DecoderOptions.None);
            var cfgBuilder = new StaticFlowGraphBuilder<Instruction>(
                instructionProvider,
                new X86StaticSuccessorResolver());

            ControlFlowGraph<Instruction> graph = cfgBuilder.ConstructFlowGraph((uint)rva);
            return graph;
        }

        public static InstructionList GetInstructionsFromGraph(ControlFlowGraph<Instruction> graph)
            => new InstructionList(graph.Nodes.SelectMany(n => n.Contents.Instructions).OrderBy(i => i.IP));

        public static byte[] EncodeBytes(InstructionList methodBody, bool is32Bit)
        {
            using var ms = new MemoryStream();
            var encoder = Encoder.Create(is32Bit ? 32 : 64, new StreamCodeWriter(ms));

            foreach (ref var instruction in methodBody)
                encoder.Encode(instruction, instruction.IP);

            return ms.ToArray();
        }
        
        public static bool TryGetJumpTarget(this Instruction instr, out ulong target)
        {
            if (instr.FlowControl != FlowControl.ConditionalBranch &&
                instr.FlowControl != FlowControl.UnconditionalBranch)
            {
                target = default;
                return false;
            }

            // short or long jumps
            if (instr.Op0Kind == OpKind.NearBranch16 || instr.Op0Kind == OpKind.NearBranch32 || instr.Op0Kind == OpKind.NearBranch64)
            {
                target = instr.NearBranchTarget;
                return true;
            }

            if (instr.Op0Kind == OpKind.FarBranch16 || instr.Op0Kind == OpKind.FarBranch32)
            {
                // TODO: check if possible
                target = default;
                return false;
            }

            target = default;
            return false;
        }
    }
}