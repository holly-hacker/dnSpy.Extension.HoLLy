using System.Collections.Generic;
using System.ComponentModel.Composition;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Text.Classification;
using dnSpy.Contracts.TreeView.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace HoLLy.dnSpyExtension.Misc
{
    internal static class TreeViewNodeColorizerClassifications
    {
        public const string UnderlineClassificationType = "dnSpy.Extension.HoLLy.UnderlineClassificationType";

        // Disable compiler warnings. The fields aren't referenced, just exported so
        // the metadata can be added to some table. The fields will always be null.
#pragma warning disable CS0169
        // Export the classes that define the name, and base types
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(UnderlineClassificationType)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        // ReSharper disable once UnassignedField.Local, InconsistentNaming
        private static ClassificationTypeDefinition? UnderlineClassificationTypeDefinition;
#pragma warning restore CS0169

        // Export the classes that define the colors and order
        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = UnderlineClassificationType)]
        [Name("Underline")]
        [UserVisible(true)]
        [Order(After = Priority.Default)]
        private sealed class UnderlineClassificationFormatDefinition : ClassificationFormatDefinition
        {
            private UnderlineClassificationFormatDefinition() => TextDecorations = System.Windows.TextDecorations.Underline;
        }
    }

    [Export(typeof(ITextClassifierProvider))]
    // You can also add more content types or use the base content type TreeViewContentTypes.TreeViewNode
    [ContentType(TreeViewContentTypes.TreeViewNodeAssemblyExplorer)]
    internal sealed class TreeViewNodeColorizerProvider : ITextClassifierProvider
    {
        private readonly IClassificationTypeRegistryService classificationTypeRegistryService;

        [ImportingConstructor]
        private TreeViewNodeColorizerProvider(IClassificationTypeRegistryService classificationTypeRegistryService) => this.classificationTypeRegistryService = classificationTypeRegistryService;

        public ITextClassifier? Create(IContentType contentType) => new TreeViewNodeColorizer(classificationTypeRegistryService);

        private class TreeViewNodeColorizer : ITextClassifier
        {
            private readonly IClassificationTypeRegistryService classificationTypeRegistryService;

            public TreeViewNodeColorizer(IClassificationTypeRegistryService classificationTypeRegistryService) => this.classificationTypeRegistryService = classificationTypeRegistryService;

            public IEnumerable<TextClassificationTag> GetTags(TextClassifierContext context)
            {
                if (!(context is TreeViewNodeClassifierContext tvContext))
                    yield break;

                // Don't do a thing if it's a tooltip
                if (tvContext.IsToolTip)
                    yield break;

                // Add the underline
                if (tvContext.Node is AssemblyDocumentNode) {
                    yield return new TextClassificationTag(new Span(0, context.Text.Length),
                        classificationTypeRegistryService.GetClassificationType(TreeViewNodeColorizerClassifications.UnderlineClassificationType));
                }
            }
        }
    }
}
