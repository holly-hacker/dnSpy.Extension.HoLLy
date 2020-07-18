using System.ComponentModel.Composition;
using dnlib.DotNet;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Documents.Tabs.DocViewer;
using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension.NativeDisassembler.Commands
{
    [ExportMenuItem(Header = "Disassemble", Group = Constants.ContextMenuGroupEdit, Order = 10000)]
    public class Disassemble : MenuItemBase
    {
        private readonly IMessageBoxService mboxService;

        [ImportingConstructor]
        public Disassemble(IMessageBoxService mboxService)
        {
            this.mboxService = mboxService;
        }
        
        public override void Execute(IMenuItemContext context)
        {
            var method = (MethodDef)context.Find<TextReference>().Reference!;
            var methodBody = IcedHelpers.ReadNativeMethodBody(method);
            mboxService.Show("Instructions: " + methodBody.Count);
        }

        public override bool IsVisible(IMenuItemContext context) => context.Find<TextReference>()?.Reference is MethodDef md && md.IsNative;
    }
}