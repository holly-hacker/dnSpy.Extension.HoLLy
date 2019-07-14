using System.ComponentModel.Composition;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Settings.Dialog;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension.SourceMap
{
    [ExportAppSettingsModifiedListener(Order = AppSettingsConstants.ORDER_LISTENER_DECOMPILER)]
    internal class SettingsModifiedListener : IAppSettingsModifiedListener
    {
        private readonly IDocumentTabService documentTabService;

        [ImportingConstructor]
        public SettingsModifiedListener(IDocumentTabService documentTabService)
        {
            this.documentTabService = documentTabService;
        }

        public void OnSettingsModified(IAppRefreshSettings appRefreshSettings)
        {
            bool shouldUpdate = appRefreshSettings.Has(Constants.SourceMapSettingsChanged);

            // TODO: only if custom decompiler is selected?

            if (shouldUpdate) {
                foreach (var document in documentTabService.DocumentTreeView.DocumentService.GetDocuments()) {
                    documentTabService.RefreshModifiedDocument(document);
                }

            }
        }
    }
}
