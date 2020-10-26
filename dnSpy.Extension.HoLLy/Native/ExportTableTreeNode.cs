using System;
using System.Collections.Generic;
using dnlib.PE;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Disassembly.Viewer;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Images;
using dnSpy.Contracts.Text;
using dnSpy.Contracts.TreeView;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension.Native
{
    public class ExportTableTreeNode : DocumentTreeNodeData, IDecompileSelf
    {
        private readonly IPEImage _peImage;
        private readonly ExportTable _exportTable;
        private readonly DisassemblyContentProviderFactory _fac;
        public override Guid Guid => Constants.AssemblyExportTableNodeGuid;
        public override NodePathName NodePathName => new NodePathName(Guid);

        public ExportTableTreeNode(IPEImage peImage, ExportTable exportTable, DisassemblyContentProviderFactory fac)
        {
            _peImage = peImage;
            _exportTable = exportTable;
            _fac = fac;
        }

        protected override ImageReference GetIcon(IDotNetImageService dnImgMgr) => DsImages.Namespace;

        protected override void WriteCore(ITextColorWriter output, IDecompiler decompiler, DocumentNodeWriteOptions options)
        {
            output.Write("Export table");
        }

        public bool Decompile(IDecompileNodeContext context)
        {
            if (!string.IsNullOrEmpty(_exportTable.Name))
                context.Output.WriteLine("Exports for " + _exportTable.Name, TextColor.Text);
            else
                context.Output.WriteLine("Exports", TextColor.Text);

            context.Output.Write("Version: ", TextColor.Text);
            context.Output.Write(_exportTable.VersionMajor.ToString(), TextColor.Number);
            context.Output.Write(".", TextColor.Text);
            context.Output.Write(_exportTable.VersionMinor.ToString(), TextColor.Number);
            context.Output.WriteLine();

            context.Output.WriteLine();

            foreach (var (address, name) in _exportTable.Exports)
            {
                context.Output.Write("RVA ", TextColor.Text);
                context.Output.Write(((uint) address).ToString("X8"), null, DecompilerReferenceFlags.None, TextColor.AsmAddress);
                context.Output.Write(": ", TextColor.Text);
                context.Output.Write(name, null, DecompilerReferenceFlags.None, TextColor.StaticMethod);
                context.Output.WriteLine();
            }

            return true;
        }

        public override IEnumerable<TreeNodeData> CreateChildren()
        {
            foreach (var (rva, name) in _exportTable.Exports)
                yield return new ExportTreeNode(_peImage, name, rva, _fac);
        }
    }
}