using System;
using dnlib.PE;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Disassembly;
using dnSpy.Contracts.Disassembly.Viewer;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Images;
using dnSpy.Contracts.Text;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.NativeDisassembler;

namespace HoLLy.dnSpyExtension.Native
{
    public class ExportTreeNode : DocumentTreeNodeData, IDecompileSelf
    {
        private readonly IPEImage _peImage;
        private readonly string _name;
        private readonly RVA _rva;
        private readonly DisassemblyContentProviderFactory _factory;

        public override Guid Guid => Constants.AssemblyExportNodeGuid;
        public override NodePathName NodePathName => new NodePathName(Guid);

        public ExportTreeNode(IPEImage peImage, string name, RVA rva, DisassemblyContentProviderFactory factory)
        {
            _peImage = peImage;
            _name = name;
            _rva = rva;
            _factory = factory;
        }

        protected override ImageReference GetIcon(IDotNetImageService dnImgMgr) => DsImages.Output;

        protected override void WriteCore(ITextColorWriter output, IDecompiler decompiler, DocumentNodeWriteOptions options)
        {
            output.Write(TextColor.AsmAddress, ((uint) _rva).ToString("X8"));
            output.Write(": ");
            output.Write(TextColor.StaticMethod, _name);
        }

        public bool Decompile(IDecompileNodeContext context)
        {
            bool is32Bit = _peImage.ImageNTHeaders.FileHeader.Machine.IsAMD64();

            var graph = IcedHelpers.ReadNativeFunction(_peImage.Filename, (uint) _peImage.ToFileOffset(_rva), is32Bit);
            var instructions = IcedHelpers.GetInstructionsFromGraph(graph);
            var encodedBytes = IcedHelpers.EncodeBytes(instructions, is32Bit);
            
            var block = new NativeCodeBlock(NativeCodeBlockKind.Code, (uint)_rva, new ArraySegment<byte>(encodedBytes), null);

            var native = new NativeCode(is32Bit ? NativeCodeKind.X86_32 : NativeCodeKind.X86_64,
                NativeCodeOptimization.Unknown, new[] {block}, null, new NativeVariableInfo[0],
                _name, _name, _peImage.Filename);

            var contentProvider = _factory.Create(native, DisassemblyContentFormatterOptions.None, null, null);
            
            foreach (var disassemblyText in contentProvider.GetContent().Text)
            {
                context.Output.Write(disassemblyText.Text, disassemblyText.Reference, (DecompilerReferenceFlags)disassemblyText.ReferenceFlags, disassemblyText.Color);
            }

            return true;
        }
    }
}