using System.Collections.Generic;
using System.ComponentModel.Composition;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.TreeView;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension.DetectItEasy
{
    [ExportTreeNodeDataProvider(Guid = DocumentTreeViewConstants.ASSEMBLY_NODE_GUID)]
    internal class DieAssemblyTreeNodeDataProvider : ITreeNodeDataProvider
    {
        private readonly Settings _settings;

        [ImportingConstructor]
        public DieAssemblyTreeNodeDataProvider(Settings settings)
        {
            _settings = settings;
        }

        public IEnumerable<TreeNodeData> Create(TreeNodeDataProviderContext context)
        {
            if (!_settings.IsDiePathValid)
                yield break;

            var data = (DsDocumentNode) context.Owner.Data;

            var peImage = data.Document.PEImage;

            if (peImage?.Filename != null)
                yield return new DieTreeNode(peImage.Filename, _settings.DiePath!);
        }
    }

    [ExportTreeNodeDataProvider(Guid = DocumentTreeViewConstants.PEDOCUMENT_NODE_GUID)]
    internal class DiePeTreeNodeDataProvider : ITreeNodeDataProvider
    {
        private readonly Settings _settings;

        public DiePeTreeNodeDataProvider(Settings settings)
        {
            _settings = settings;
        }

        public IEnumerable<TreeNodeData> Create(TreeNodeDataProviderContext context)
        {
            if (!_settings.IsDiePathValid)
                yield break;

            var data = (DsDocumentNode) context.Owner.Data;

            var peImage = data.Document.PEImage;

            if (peImage?.Filename != null)
                yield return new DieTreeNode(peImage.Filename, _settings.DiePath!);
        }
    }
}