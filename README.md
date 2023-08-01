dnSpy.Extension.HoLLy
=====================

A [dnSpyEx](https://github.com/dnSpyEx/dnSpy) extension to aid reversing of obfuscated assemblies.

### Features
- **Change the displayed symbol name of types, methods, properties or fields, without modifying the binary.** These modified names are saved in an xml file, meaning you can write a tool to generate them automatically.
	- Please keep in mind that this works in a relatively hacky way, and it can't be seen as a perfect replacement for manually renaming symbols. See [current issues](https://github.com/HoLLy-HaCKeR/dnSpy.Extension.HoLLy/labels/area%3Asourcemap) for limitations.
	- This can be accessed through the decompiler language dropdown in the menu bar.
- **Inject managed (.NET) DLLs into the debugged process.** The injected DLL must have a method with signature `static int Method(string argument)`. .NET Core and Unity x64 are not yet supported.
- **Disassemble native functions**
- **Show control flow graphs for both managed and native functions**
- Underline managed assemblies in the treeview.
- Several commands to help with extension development in debug mode

### Other extensions
I have developed some other extensions which are linked here for convenience:
- [dnSpy.Extension.DiscordRPC](https://github.com/HoLLy-HaCKeR/dnSpy.Extension.DiscordRPC/tree/master)
- [dnSpy.Extension.ThemeHotReload](https://github.com/HoLLy-HaCKeR/dnSpy.Extension.ThemeHotReload/tree/master)

### Installation
Download the [latest release](https://github.com/holly-hacker/dnspy.extension.holly/releases/latest) for your dnSpyEx version (`net48` or `net6.0-windows`) and extract it to the `bin/Extensions/dnSpy.Extensions.HoLLy` directory. You may need to create this folder.

Make sure that you copied all the dependency DLLs too. Your directory structure will look something like this:
```
dnSpy-net-win64/
├─ dnSpy.exe
├─ dnSpy.Console.exe
└─ bin/
  ├─ Extensions/
  │ └─ dnSpy.Extension.HoLLy/
  │   ├─ AutomaticGraphLayout.dll
  │   ├─ dnSpy.Extension.HoLLy.x.dll
  │   ├─ Echo.Core.dll
  │   └─ ...
  ├─ LicenseInfo/
  ├─ FileLists/
  ├─ Themes/
  ├─ dnSpy.Analyzer.x.dll
  ├─ dnSpy.Contracts.Debugger.dll
  └─ ...
```

Also make sure that you are using the correct version of dnSpy that matches the plugin! This should be mentioned in the [release notes](https://github.com/holly-hacker/dnspy.extension.holly/releases/latest) or the [changelog](https://github.com/HoLLy-HaCKeR/dnSpy.Extension.HoLLy/blob/master/CHANGELOG.md).
The plugin **will not work** with certain mismatched versions due to strong-name signing of some dependencies.

### Developing
To test the extension, you can launch dnSpy with the `--extension-directory {directory}` argument, where `{directory}` is the build directory (ie. `.../bin/Debug/netcoreapp3.1`).
JetBrains Rider supports launch profiles, allowing you to specify dnSpy as the executable to start. This means you can launch and debug the extension from within the IDE.

Due to how the .NET Framework does assembly resolving, this method may only work on .NET Core.

### License
Due to dnSpy being licensed under the GPLv3 license, this plugin is too.

### Used libraries
- [dnSpyEx](https://github.com/dnSpyEx/dnSpy) and its [dependencies](https://github.com/dnSpyEx/dnSpy#list-of-other-open-source-libraries-used-by-dnspy), licensed under the [GPLv3 license](https://github.com/dnSpyEx/dnSpy/blob/master/dnSpy/dnSpy/LicenseInfo/LICENSE.txt) and [others](https://github.com/dnSpyEx/dnSpy/tree/master/dnSpy/dnSpy/LicenseInfo)
- [iced](https://github.com/0xd4d/iced), licensed under the [MIT license](https://github.com/0xd4d/iced/blob/master/LICENSE.txt)
- [dnlib](https://github.com/0xd4d/dnlib), licensed under the [MIT license](https://github.com/0xd4d/dnlib/blob/master/LICENSE.txt)
- [Echo](https://github.com/Washi1337/Echo), licensed under the [LGPLv3 license](https://github.com/Washi1337/Echo/blob/master/LICENSE.md)
- [Microsoft Automatic Graph Layout](https://github.com/microsoft/automatic-graph-layout), licensed under the [MIT license](https://github.com/microsoft/automatic-graph-layout/blob/master/LICENSE)
