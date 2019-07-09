using dnSpy.Contracts.Menus;
using HoLLy.dnSpyExtension.Common;

namespace HoLLy.dnSpyExtension.SourceMap.Commands
{
    [ExportMenu(Header = "_SourceMap", Guid = Constants.AppMenuGroupSourceMap, OwnerGuid = MenuConstants.APP_MENU_GUID, Order = 50000)]
    internal class SourceMapAppMenu : IMenu { }
}
