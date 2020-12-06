using System.Collections.Generic;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.TreeView;

namespace HoLLy.dnSpyExtension.DetectItEasy
{
    [ExportTreeNodeDataProvider(Guid = DocumentTreeViewConstants.ASSEMBLY_NODE_GUID)]
    public class DieAssemblyTreeNodeDataProvider : ITreeNodeDataProvider
    {
        public IEnumerable<TreeNodeData> Create(TreeNodeDataProviderContext context)
        {
            var data = (DsDocumentNode) context.Owner.Data;

            var peImage = data.Document.PEImage;

            if (peImage?.Filename != null)
                yield return new DieTreeNode(peImage.Filename);
        }
    }

    [ExportTreeNodeDataProvider(Guid = DocumentTreeViewConstants.PEDOCUMENT_NODE_GUID)]
    public class DiePeTreeNodeDataProvider : ITreeNodeDataProvider
    {
        public IEnumerable<TreeNodeData> Create(TreeNodeDataProviderContext context)
        {
            var data = (DsDocumentNode) context.Owner.Data;

            var peImage = data.Document.PEImage;

            if (peImage?.Filename != null)
                yield return new DieTreeNode(peImage.Filename);
        }
    }
}