using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Text;
using HoLLy.dnSpy.Extension.SourceMap;

namespace HoLLy.dnSpy.Extension.Decompilers.Decorators
{
    internal class DecompilerDecorator : IDecompiler
    {
        private readonly IDecompiler decompiler;
        private readonly ISourceMapStorage sourceMap;

        public DecompilerDecorator(IDecompiler decompiler, ISourceMapStorage sourceMap)
        {
            this.decompiler = decompiler;
            this.sourceMap = sourceMap;
        }

        private IDecompilerOutput GetOutput(IDecompilerOutput output)
        {
            return new DecompilerOutputDecorator(output, sourceMap);
        }

        public DecompilerSettingsBase Settings => decompiler.Settings;
        public MetadataTextColorProvider MetadataTextColorProvider => decompiler.MetadataTextColorProvider;
        public string ContentTypeString => decompiler.ContentTypeString;
        public string GenericNameUI => decompiler.GenericNameUI;
        public string UniqueNameUI => decompiler.UniqueNameUI + " - Decorated";
        public double OrderUI => decompiler.OrderUI + double.Epsilon * 10;
        public Guid GenericGuid => decompiler.GenericGuid;
        public Guid UniqueGuid => decompiler.UniqueGuid.XorGuid(Constants.DecompilerGuid);
        public string FileExtension => decompiler.FileExtension;
        public string ProjectFileExtension => decompiler.ProjectFileExtension;

        public void WriteName(ITextColorWriter output, TypeDef type) => decompiler.WriteName(output, type);
        public void WriteName(ITextColorWriter output, PropertyDef property, bool? isIndexer) => decompiler.WriteName(output, property, isIndexer);
        public void WriteType(ITextColorWriter output, ITypeDefOrRef type, bool includeNamespace, ParamDef pd = null) => decompiler.WriteType(output, type, includeNamespace, pd);
        public void WriteToolTip(ITextColorWriter output, IMemberRef member, IHasCustomAttribute typeAttributes) => decompiler.WriteToolTip(output, member, typeAttributes);
        public void WriteToolTip(ITextColorWriter output, ISourceVariable variable) => decompiler.WriteToolTip(output, variable);
        public void WriteNamespaceToolTip(ITextColorWriter output, string @namespace) => decompiler.WriteNamespaceToolTip(output, @namespace);
        public void Write(ITextColorWriter output, IMemberRef member, FormatterOptions flags) => decompiler.Write(output, member, flags);
        public void WriteCommentBegin(IDecompilerOutput output, bool addSpace) => decompiler.WriteCommentBegin(output, addSpace);
        public void WriteCommentEnd(IDecompilerOutput output, bool addSpace) => decompiler.WriteCommentEnd(output, addSpace);

        public void Decompile(MethodDef method, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(method, GetOutput(output), ctx);
        public void Decompile(PropertyDef property, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(property, GetOutput(output), ctx);
        public void Decompile(FieldDef field, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(field, GetOutput(output), ctx);
        public void Decompile(EventDef ev, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(ev, GetOutput(output), ctx);
        public void Decompile(TypeDef type, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(type, GetOutput(output), ctx);
        public void DecompileNamespace(string @namespace, IEnumerable<TypeDef> types, IDecompilerOutput output, DecompilationContext ctx) => decompiler.DecompileNamespace(@namespace, types, GetOutput(output), ctx);
        public void Decompile(AssemblyDef asm, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(asm, GetOutput(output), ctx);
        public void Decompile(ModuleDef mod, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(mod, GetOutput(output), ctx);
        public bool ShowMember(IMemberRef member) => decompiler.ShowMember(member);
        public bool CanDecompile(DecompilationType decompilationType) => decompiler.CanDecompile(decompilationType);
        public void Decompile(DecompilationType decompilationType, object data) => decompiler.Decompile(decompilationType, data);
    }
}
