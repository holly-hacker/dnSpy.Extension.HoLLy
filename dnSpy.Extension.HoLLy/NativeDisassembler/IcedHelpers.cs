using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnSpy.Contracts.Decompiler;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.Platforms.Iced;
using Iced.Intel;

namespace HoLLy.dnSpyExtension.NativeDisassembler
{
    public static class IcedHelpers
    {
        public static byte[] ReadNativeMethodBodyBytes(MethodDef method)
            => EncodeBytes(ReadNativeMethodBody(method), method.Module.IsAMD64 ? 64 : 32);
        
        public static InstructionList ReadNativeMethodBody(MethodDef method)
        {
            var mod = method.Module;
            var loc = mod.Location;
            bool is32Bit = !method.Module.IsAMD64;
            var rva = (uint)method.NativeBody.RVA;
            var fileOffset = mod.ToFileOffset(rva)!.Value;
            
            return ReadNativeFunction(loc, fileOffset, is32Bit);
        }

        public static InstructionList ReadNativeFunction(string loc, uint fileOffset, bool is32Bit)
        {
            using var fs = File.OpenRead(loc);
            fs.Position = fileOffset;

            var architecture = new X86Architecture();
            var instructionProvider = new X86DecoderInstructionProvider(architecture, fs, is32Bit ? 32 : 64);
            var cfgBuilder = new StaticFlowGraphBuilder<Instruction>(
                instructionProvider,
                new X86StaticSuccessorResolver());

            // pass in a file offset, since we're working on a file on disk. would pass rva (and base addr in provider
            // ctor) for in-memory.
            var graph = cfgBuilder.ConstructFlowGraph(fileOffset);
            return new InstructionList(graph.Nodes.SelectMany(n => n.Contents.Instructions).OrderBy(i => i.IP));
        }

        public static byte[] EncodeBytes(InstructionList methodBody, int bitness)
        {
            using var ms = new MemoryStream();
            var encoder = Encoder.Create(bitness, new StreamCodeWriter(ms));

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