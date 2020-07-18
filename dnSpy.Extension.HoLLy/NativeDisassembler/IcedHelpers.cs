using System;
using System.IO;
using dnlib.DotNet;
using dnSpy.Contracts.Decompiler;
using Iced.Intel;

namespace HoLLy.dnSpyExtension.NativeDisassembler
{
    public static class IcedHelpers
    {
        public static InstructionList ReadNativeMethodBody(MethodDef method)
        {
            // TODO: use Echo for this
            var mod = method.Module;
            var rva = (uint)method.NativeBody.RVA;
            var fileOffset = mod.ToFileOffset(rva)!.Value;
            
            using var fs = File.OpenRead(mod.Location);
            fs.Position = fileOffset;
            var reader = new StreamCodeReader(fs);
            var decoder = Decoder.Create(mod.Is32BitRequired ? 32 : 64, reader);
            decoder.IP = rva;

            // decode loop
            Instruction instruction;
            var instructionList = new InstructionList();
            ulong minAddress = ulong.MinValue;
            do
            {
                decoder.Decode(out instruction);
                if (instruction.TryGetJumpTarget(out var target))
                    minAddress = Math.Max(minAddress, target);
                
                instructionList.Add(instruction);
            } while (!(instruction.FlowControl == FlowControl.Return && decoder.IP >= minAddress));

            return instructionList;
        }

        public static byte[] EncodeBytes(InstructionList methodBody, int bitness)
        {
            using var ms = new MemoryStream();
            var encoder = Encoder.Create(bitness, new StreamCodeWriter(ms));

            foreach (ref var instruction in methodBody)
                encoder.Encode(instruction, instruction.IP);

            var encodedBytes = ms.ToArray();
            return encodedBytes;
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