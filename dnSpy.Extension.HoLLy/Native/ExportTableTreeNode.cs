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
        private readonly IPEImage peImage;
        private readonly ExportTable exportTable;
        private readonly DisassemblyContentProviderFactory fac;
        public override Guid Guid => Constants.AssemblyExportTableNodeGuid;
        public override NodePathName NodePathName => new NodePathName(Guid);

        public ExportTableTreeNode(IPEImage peImage, ExportTable exportTable, DisassemblyContentProviderFactory fac)
        {
            this.peImage = peImage;
            this.exportTable = exportTable;
            this.fac = fac;
        }

        protected override ImageReference GetIcon(IDotNetImageService dnImgMgr) => DsImages.Namespace;

        protected override void WriteCore(ITextColorWriter output, IDecompiler decompiler, DocumentNodeWriteOptions options)
        {
            output.Write(BoxedTextColor.Text, "Export table");
        }

        public bool Decompile(IDecompileNodeContext context)
        {
            if (!string.IsNullOrEmpty(exportTable.Name))
                context.Output.WriteLine("Exports for " + exportTable.Name, TextColor.Text);
            else
                context.Output.WriteLine("Exports", TextColor.Text);

            context.Output.Write("Version: ", TextColor.Text);
            context.Output.Write(exportTable.VersionMajor.ToString(), TextColor.Number);
            context.Output.Write(".", TextColor.Text);
            context.Output.Write(exportTable.VersionMinor.ToString(), TextColor.Number);
            context.Output.WriteLine();

            context.Output.WriteLine();

            foreach (var (address, name) in exportTable.Exports)
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
            foreach (var (rva, name) in exportTable.Exports)
                yield return new ExportTreeNode(peImage, name, rva, fac);
        }
    }
}