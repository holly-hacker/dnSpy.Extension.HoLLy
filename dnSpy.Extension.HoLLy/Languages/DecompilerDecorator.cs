using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Text;

namespace HoLLy.dnSpy.Extension.Languages
{
    public class DecompilerDecorator : IDecompiler
    {
        private readonly IDecompiler decompiler;

        public DecompilerDecorator(IDecompiler decompiler)
        {
            this.decompiler = decompiler;
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
        public void Decompile(MethodDef method, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(method, output, ctx);
        public void Decompile(PropertyDef property, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(property, output, ctx);
        public void Decompile(FieldDef field, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(field, output, ctx);
        public void Decompile(EventDef ev, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(ev, output, ctx);
        public void Decompile(TypeDef type, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(type, output, ctx);
        public void DecompileNamespace(string @namespace, IEnumerable<TypeDef> types, IDecompilerOutput output, DecompilationContext ctx) => decompiler.DecompileNamespace(@namespace, types, output, ctx);
        public void Decompile(AssemblyDef asm, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(asm, output, ctx);
        public void Decompile(ModuleDef mod, IDecompilerOutput output, DecompilationContext ctx) => decompiler.Decompile(mod, output, ctx);
        public void WriteToolTip(ITextColorWriter output, IMemberRef member, IHasCustomAttribute typeAttributes) => decompiler.WriteToolTip(output, member, typeAttributes);
        public void WriteToolTip(ITextColorWriter output, ISourceVariable variable) => decompiler.WriteToolTip(output, variable);
        public void WriteNamespaceToolTip(ITextColorWriter output, string @namespace) => decompiler.WriteNamespaceToolTip(output, @namespace);
        public void Write(ITextColorWriter output, IMemberRef member, FormatterOptions flags) => decompiler.Write(output, member, flags);
        public void WriteCommentBegin(IDecompilerOutput output, bool addSpace) => decompiler.WriteCommentBegin(output, addSpace);
        public void WriteCommentEnd(IDecompilerOutput output, bool addSpace) => decompiler.WriteCommentEnd(output, addSpace);
        public bool ShowMember(IMemberRef member) => decompiler.ShowMember(member);
        public bool CanDecompile(DecompilationType decompilationType) => decompiler.CanDecompile(decompilationType);
        public void Decompile(DecompilationType decompilationType, object data) => decompiler.Decompile(decompilationType, data);
    }
}
