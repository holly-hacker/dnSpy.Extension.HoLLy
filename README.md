dnSpy.Extension.HoLLy
=====================

A dnSpy extension to aid reversing of obfuscated assemblies.

### Features
- **Change the displayed symbol name of types, methods, properties or fields, without modifying the binary.** These modified names are saved in an xml file, meaning you can write a tool to generate them automatically.
- **Inject managed (.NET) DLLs into the debugged process.** The injected DLL must have a method with signature `static int Method(string argument)`, can not have a conflicting architecture (no x86-only in an x64 process, AnyCPU is fine) and can not have an incompatible framework version (no .NET 4.7.2 in a .NET 4.0 process, or .NET 2.0 in a .NET 4.0 process due to CLR version differences). .NET Core and Unity are not yet supported.
- Underline managed assemblies in the treeview.

### License
Because dnSpy is licensed under the GPLv3 license, this plugin has to be as well. I like it as little as you do :(

### Attribution
- [dnSpy](https://github.com/0xd4d/dnSpy) and its dependencies, licensed under the [GPLv3 license](https://github.com/0xd4d/dnSpy/blob/master/dnSpy/dnSpy/LicenseInfo/LICENSE.txt)
- [iced](https://github.com/0xd4d/iced), licensed under the [MIT license](https://github.com/0xd4d/iced/blob/master/LICENSE.txt)
- [dnlib](https://github.com/0xd4d/dnlib), licensed under the [MIT license](https://github.com/0xd4d/dnlib/blob/master/LICENSE.txt)