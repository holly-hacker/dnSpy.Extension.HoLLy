using System;
using System.ComponentModel.Composition;
using dnlib.PE;
using dnSpy.Contracts.Disassembly;
using dnSpy.Contracts.Disassembly.Viewer;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Menus;
using dnSpy.Contracts.TreeView;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.Commands;

namespace HoLLy.dnSpyExtension.NativeDisassembler.Commands
{
    [ExportMenuItem(Header = "Disassemble Entrypoint", Group = Constants.ContextMenuGroupEdit, Order = 10000)]
    public class DisassembleNativeEntrypoint : SingleTreeViewMenuItemBase
    {
        private readonly DisassemblyContentProviderFactory fac;
        private readonly Lazy<DisassemblyViewerService> disassemblyViewerService;

        [ImportingConstructor]
        public DisassembleNativeEntrypoint(DisassemblyContentProviderFactory fac, Lazy<DisassemblyViewerService> disassemblyViewerService)
        {
            this.fac = fac;
            this.disassemblyViewerService = disassemblyViewerService;
        }
        
        public override bool IsVisible(TreeNodeData context) =>
            context is AssemblyDocumentNode node && node.Document.PEImage != null;

        public override void Execute(TreeNodeData context)
        {
            var node = (AssemblyDocumentNode)context;
            var pe = node.Document.PEImage!;
            bool is32Bit = pe.ImageNTHeaders.FileHeader.Machine.IsAMD64();

            var rvaStart = pe.ImageNTHeaders.OptionalHeader.AddressOfEntryPoint;

            var graph = IcedHelpers.ReadNativeFunction(node.Document.Filename, (uint) pe.ToFileOffset(rvaStart), is32Bit);
            var instructions = IcedHelpers.GetInstructionsFromGraph(graph);
            var encodedBytes = IcedHelpers.EncodeBytes(instructions, is32Bit);
            
            var block = new NativeCodeBlock(NativeCodeBlockKind.Code, (uint)rvaStart, new ArraySegment<byte>(encodedBytes), null);

            var native = new NativeCode(is32Bit ? NativeCodeKind.X86_32 : NativeCodeKind.X86_64,
                NativeCodeOptimization.Unknown, new[] {block}, null, new NativeVariableInfo[0],
                "Native Entrypoint", "native entry", node.Document.Filename);

            var contentProvider = fac.Create(native, DisassemblyContentFormatterOptions.None, null, null);
            disassemblyViewerService.Value.Show(contentProvider, true);
        }
    }
}