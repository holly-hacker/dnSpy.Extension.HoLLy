using System;
using System.ComponentModel.Composition;
using dnlib.DotNet;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Disassembly;
using dnSpy.Contracts.Disassembly.Viewer;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension.NativeDisassembler.Commands
{
    [ExportMenuItem(Header = "Disassemble", Group = Constants.ContextMenuGroupEdit, Order = 10000)]
    public class DisassembleNativeMethod : MenuItemBase
    {
        private readonly DisassemblyContentProviderFactory fac;
        private readonly Lazy<DisassemblyViewerService> disassemblyViewerService;

        [ImportingConstructor]
        public DisassembleNativeMethod(DisassemblyContentProviderFactory fac, Lazy<DisassemblyViewerService> disassemblyViewerService)
        {
            this.fac = fac;
            this.disassemblyViewerService = disassemblyViewerService;
        }
        
        public override void Execute(IMenuItemContext context)
        {
            var method = (MethodDef)context.Find<TextReference>().Reference!;
            var encodedBytes = IcedHelpers.ReadNativeMethodBodyBytes(method);
            var is32Bit = !method.Module.IsAMD64;

            var block = new NativeCodeBlock(NativeCodeBlockKind.Code, (uint)method.NativeBody.RVA, new ArraySegment<byte>(encodedBytes), null);
            var vars = new NativeVariableInfo[method.Parameters.Count];
            for (var i = 0; i < method.Parameters.Count; i++)
                vars[i] = new NativeVariableInfo(false, i, method.Parameters[i].Name);

            var native = new NativeCode(is32Bit ? NativeCodeKind.X86_32 : NativeCodeKind.X86_64,
                NativeCodeOptimization.Unknown, new[] {block}, null, vars,
                method.FullName, IdentifierEscaper.Escape(method.Name), method.Module.Name);

            var contentProvider = fac.Create(native, DisassemblyContentFormatterOptions.None, null, null);
            disassemblyViewerService.Value.Show(contentProvider, true);
        }

        public override bool IsVisible(IMenuItemContext context) => context.Find<TextReference>().Reference is MethodDef { IsNative: true };
    }
}