using dnSpy.Contracts.Menus;

namespace HoLLy.dnSpyExtension.Commands.SourceMap
{
    [ExportMenu(Header = "_SourceMap", Guid = Constants.AppMenuGroupSourceMap, OwnerGuid = MenuConstants.APP_MENU_GUID, Order = 50000)]
    internal class SourceMapAppMenu : IMenu { }
}
