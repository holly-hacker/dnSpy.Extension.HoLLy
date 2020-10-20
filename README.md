dnSpy.Extension.HoLLy
=====================

A [dnSpy](https://github.com/dnSpy/dnSpy) extension to aid reversing of obfuscated assemblies.

### Features
- **Change the displayed symbol name of types, methods, properties or fields, without modifying the binary.** These modified names are saved in an xml file, meaning you can write a tool to generate them automatically.
	- Please keep in mind that this works in a relatively hacky way, and it can't be seen as a perfect replacement for manually renaming symbols. See [current issues](https://github.com/HoLLy-HaCKeR/dnSpy.Extension.HoLLy/labels/area%3Asourcemap) for limitations.
- **Inject managed (.NET) DLLs into the debugged process.** The injected DLL must have a method with signature `static int Method(string argument)`. .NET Core and Unity x64 are not yet supported.
- **Disassemble native functions**
- **Show control flow graphs for both managed and native functions**
- Underline managed assemblies in the treeview.
- Several commands to help with extension development in debug mode

### Installation
Download the [latest release](https://github.com/holly-hacker/dnspy.extension.holly/releases/latest) for your dnSpy version (net472 or netcoreapp3.1) and extract it to the `bin/Extensions/dnSpy.Extensions.HoLLy` directory. You may need to create this folder.

Make sure that you copied all the dependency DLLs too. Your directory structure will look something like this:
```
dnSpy-netcore-win64/
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
To test the extension, you can launch dnSpy with the `--extension-directory {direcory}` argument, where `{directory}` is the build directory (ie. `.../bin/Release/net472`).
JetBrains Rider supports launch profiles, allowing you to specify dnSpy as the executable to start. This means you can launch and debug the extension from within the IDE.

### License
Due to dnSpy being licensed under the GPLv3 license, this plugin is too.

### Used libraries
- [dnSpy](https://github.com/0xd4d/dnSpy) and its [dependencies](https://github.com/dnSpy/dnSpy#list-of-other-open-source-libraries-used-by-dnspy), licensed under the [GPLv3 license](https://github.com/0xd4d/dnSpy/blob/master/dnSpy/dnSpy/LicenseInfo/LICENSE.txt) and [others](https://github.com/dnSpy/dnSpy/tree/master/dnSpy/dnSpy/LicenseInfo)
- [iced](https://github.com/0xd4d/iced), licensed under the [MIT license](https://github.com/0xd4d/iced/blob/master/LICENSE.txt)
- [dnlib](https://github.com/0xd4d/dnlib), licensed under the [MIT license](https://github.com/0xd4d/dnlib/blob/master/LICENSE.txt)
- [Echo](https://github.com/Washi1337/Echo), licensed under the [LGPLv3 license](https://github.com/Washi1337/Echo/blob/master/LICENSE.md)
- [Microsoft Automatic Graph Layout](https://github.com/microsoft/automatic-graph-layout), licensed under the [MIT license](https://github.com/microsoft/automatic-graph-layout/blob/master/LICENSE)
