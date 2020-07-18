using System;
using System.ComponentModel.Composition;
using dnlib.DotNet;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Disassembly;
using dnSpy.Contracts.Disassembly.Viewer;
using dnSpy.Contracts.Documents.Tabs;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension.NativeDisassembler.Commands
{
    [ExportMenuItem(Header = "Disassemble", Group = Constants.ContextMenuGroupEdit, Order = 10000)]
    public class Disassemble : MenuItemBase
    {
        private readonly IMessageBoxService mboxService;
        private readonly DisassemblyContentProviderFactory fac;
        private readonly IDocumentTabService tabService;
        private readonly Lazy<DisassemblyViewerService> disassemblyViewerService;

        [ImportingConstructor]
        public Disassemble(IMessageBoxService mboxService, DisassemblyContentProviderFactory fac, IDocumentTabService tabService, Lazy<DisassemblyViewerService> disassemblyViewerService)
        {
            this.mboxService = mboxService;
            this.fac = fac;
            this.tabService = tabService;
            this.disassemblyViewerService = disassemblyViewerService;
        }
        
        public override void Execute(IMenuItemContext context)
        {
            var method = (MethodDef)context.Find<TextReference>().Reference!;
            var methodBody = IcedHelpers.ReadNativeMethodBody(method);
            var encodedBytes = IcedHelpers.EncodeBytes(methodBody, method.Module.Is32BitRequired ? 32 : 64);

            var block = new NativeCodeBlock(NativeCodeBlockKind.Unknown, (uint)method.NativeBody.RVA, new ArraySegment<byte>(encodedBytes), null);
            var vars = new NativeVariableInfo[0]; // TODO: argument variables

            var native = new NativeCode(method.Module.Is32BitRequired ? NativeCodeKind.X86_32 : NativeCodeKind.X86_64,
                NativeCodeOptimization.Unknown, new[] {block}, null, vars,
                method.FullName, method.Name, method.Module.Name);

            var contentProvider = fac.Create(native, DisassemblyContentFormatterOptions.None, null, null);
            disassemblyViewerService.Value.Show(contentProvider, true);
        }

        public override bool IsVisible(IMenuItemContext context) => context.Find<TextReference>()?.Reference is MethodDef md && md.IsNative;
    }
}