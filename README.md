dnSpy.Extension.HoLLy
=====================

A dnSpy extension to aid reversing of obfuscated assemblies.

### Features
- **Change the displayed symbol name of types, methods, properties or fields, without modifying the binary.** These modified names are saved in an xml file, meaning you can write a tool to generate them automatically.
	- Please keep in mind that this works in a relatively hacky way, and it can't be seen as a perfect replacement for manually renaming symbols. See [current issues](https://github.com/HoLLy-HaCKeR/dnSpy.Extension.HoLLy/labels/area%3Asourcemap) for limitations.
- **Inject managed (.NET) DLLs into the debugged process.** The injected DLL must have a method with signature `static int Method(string argument)`. .NET Core and Unity x64 are not yet supported.
- **Disassemble native functions**
- Underline managed assemblies in the treeview.
- Several commands to help with extension development in debug mode

### License
Due to dnSpy being licensed under the GPLv3 license, this plugin is too.

### Used libraries
- [dnSpy](https://github.com/0xd4d/dnSpy) and its dependencies, licensed under the [GPLv3 license](https://github.com/0xd4d/dnSpy/blob/master/dnSpy/dnSpy/LicenseInfo/LICENSE.txt)
- [iced](https://github.com/0xd4d/iced), licensed under the [MIT license](https://github.com/0xd4d/iced/blob/master/LICENSE.txt)
- [dnlib](https://github.com/0xd4d/dnlib), licensed under the [MIT license](https://github.com/0xd4d/dnlib/blob/master/LICENSE.txt)
- [Echo](https://github.com/Washi1337/Echo), licensed under the [LGPLv3 license](https://github.com/Washi1337/Echo/blob/master/LICENSE.md)
