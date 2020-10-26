using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using dnSpy.Contracts.Disassembly.Viewer;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.TreeView;

namespace HoLLy.dnSpyExtension.Native
{
    [ExportTreeNodeDataProvider(Guid = DocumentTreeViewConstants.PEDOCUMENT_NODE_GUID)]
    public class NativeAssemblyTreeNodeDataProvider : ITreeNodeDataProvider
    {
        private readonly DisassemblyContentProviderFactory _fac;

        [ImportingConstructor]
        public NativeAssemblyTreeNodeDataProvider(DisassemblyContentProviderFactory fac)
        {
            _fac = fac;
        }

        public IEnumerable<TreeNodeData> Create(TreeNodeDataProviderContext context)
        {
            // PEDocument is only for native assemblies, not managed ones.
            var data = (PEDocumentNode)context.Owner.Data;

            var peImage = data.Document.PEImage;

            if (peImage is { })
            {
                var dataDirectories = peImage.ImageNTHeaders.OptionalHeader.DataDirectories;
                Debug.Assert(dataDirectories.Length == 16);
                var exportTableDirectory = dataDirectories[0]!;

                if (exportTableDirectory.Size != 0)
                {
                    var exportTable = ExportTable.Read(peImage.DataReaderFactory, peImage, exportTableDirectory);
                    yield return new ExportTableTreeNode(peImage, exportTable, _fac);
                }
            }
        }
    }
}