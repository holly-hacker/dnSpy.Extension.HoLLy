using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Text;
using HoLLy.dnSpyExtension.Common;
using HoLLy.dnSpyExtension.Common.SourceMap;

namespace HoLLy.dnSpyExtension.SourceMap.Decompilers
{
    internal class SourceMapDecompilerDecorator : IDecompiler
    {
        private const string UniqueNameFormat = "{0} (w/ SourceMap)";

        private readonly IDecompiler implementation;
        private readonly ISourceMapStorage sourceMap;

        public SourceMapDecompilerDecorator(IDecompiler implementation, ISourceMapStorage sourceMap)
        {
            this.implementation = implementation;
            this.sourceMap = sourceMap;
        }

        private IDecompilerOutput GetOutput(IDecompilerOutput output) => new SourceMapDecompilerOutputDecorator(output, sourceMap);

        public DecompilerSettingsBase Settings => implementation.Settings;
        public MetadataTextColorProvider MetadataTextColorProvider => implementation.MetadataTextColorProvider;
        public string ContentTypeString => implementation.ContentTypeString;
        public string GenericNameUI => implementation.GenericNameUI;
        public string UniqueNameUI => string.Format(UniqueNameFormat, implementation.UniqueNameUI);
        public double OrderUI => implementation.OrderUI + 0.5;
        public Guid GenericGuid => implementation.GenericGuid;
        public Guid UniqueGuid => implementation.UniqueGuid.XorGuid(Constants.DecompilerGuid);
        public string FileExtension => implementation.FileExtension;
        public string? ProjectFileExtension => implementation.ProjectFileExtension;

        public void WriteName(ITextColorWriter output, TypeDef type) => implementation.WriteName(output, type);
        public void WriteName(ITextColorWriter output, PropertyDef property, bool? isIndexer) => implementation.WriteName(output, property, isIndexer);
        public void WriteType(ITextColorWriter output, ITypeDefOrRef? type, bool includeNamespace, IHasCustomAttribute? customAttributeProvider = null) => implementation.WriteType(output, type, includeNamespace, customAttributeProvider);
        public void WriteToolTip(ITextColorWriter output, IMemberRef member, IHasCustomAttribute? typeAttributes) => implementation.WriteToolTip(output, member, typeAttributes);
        public void WriteToolTip(ITextColorWriter output, ISourceVariable variable) => implementation.WriteToolTip(output, variable);
        public void WriteNamespaceToolTip(ITextColorWriter output, string? @namespace) => implementation.WriteNamespaceToolTip(output, @namespace);
        public void Write(ITextColorWriter output, IMemberRef member, FormatterOptions flags) => implementation.Write(output, member, flags);
        public void WriteCommentBegin(IDecompilerOutput output, bool addSpace) => implementation.WriteCommentBegin(output, addSpace);
        public void WriteCommentEnd(IDecompilerOutput output, bool addSpace) => implementation.WriteCommentEnd(output, addSpace);

        public void Decompile(MethodDef method, IDecompilerOutput output, DecompilationContext ctx) => implementation.Decompile(method, GetOutput(output), ctx);
        public void Decompile(PropertyDef property, IDecompilerOutput output, DecompilationContext ctx) => implementation.Decompile(property, GetOutput(output), ctx);
        public void Decompile(FieldDef field, IDecompilerOutput output, DecompilationContext ctx) => implementation.Decompile(field, GetOutput(output), ctx);
        public void Decompile(EventDef ev, IDecompilerOutput output, DecompilationContext ctx) => implementation.Decompile(ev, GetOutput(output), ctx);
        public void Decompile(TypeDef type, IDecompilerOutput output, DecompilationContext ctx) => implementation.Decompile(type, GetOutput(output), ctx);
        public void DecompileNamespace(string @namespace, IEnumerable<TypeDef> types, IDecompilerOutput output, DecompilationContext ctx) => implementation.DecompileNamespace(@namespace, types, GetOutput(output), ctx);
        public void Decompile(AssemblyDef asm, IDecompilerOutput output, DecompilationContext ctx) => implementation.Decompile(asm, GetOutput(output), ctx);
        public void Decompile(ModuleDef mod, IDecompilerOutput output, DecompilationContext ctx) => implementation.Decompile(mod, GetOutput(output), ctx);
        public bool ShowMember(IMemberRef member) => implementation.ShowMember(member);
        public bool CanDecompile(DecompilationType decompilationType) => implementation.CanDecompile(decompilationType);
        public void Decompile(DecompilationType decompilationType, object data) => implementation.Decompile(decompilationType, data);
    }
}
