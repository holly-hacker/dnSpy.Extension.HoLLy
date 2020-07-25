# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]
### Added
- **Control Flow Graphs for managed and native methods**

## [0.4.1] - 2020-07-24
### Fixed
- Native disassembly used incorrect values for architecture, causing disassembly to fail in some cases

## [0.4.0] - 2020-07-24
### Added
- **Unity x86 DLL injection (#23)**
- **Disassemble native functions and the native entrypoint**
- Debug command for showing tree node type

### Changed
- **Targets dnSpy v6.0.16, both .NET 4.7.2 and .NET Core**
- **Include [Echo](https://github.com/washi1337/echo) for better disassembling of native functions without known length**
  - The plugin will need all the included Echo DLLs to be present.
- Namespace and Type are now stored seperately in DLL injection config files, meaning old ones will be invalidated
- Improved debug command for injecting into arbitrary process
  - It now asks for runtime type
  - It now verifies the input PID while you're typing
  - Automatically assumes process architecture matches (it wouldn't work otherwise)
- "Change Displayed Name" command only appears when a sourcemapping compiler is selected

### Fixed
- Decompiler crash when user tries to sourcemap names containing invalid characters, ie. < 0x20 (#30)
- Sourcemapper tried to map "new" keywords

## [0.3.0] - 2019-07-14
### Added
- Import/export sourcemaps
  - Location with cached sourcemaps can be opened with a menu item (#5)
- Allow automatically sourcemapping DLLImports, if they don't already have a name (#11)
- Allow copying injected DLL to temporary directory to avoid locking the file (#2)
- Store list of recent injections for easy repeated use (#4)

### Fixed
- Constructors now get correctly sourcemapped (#13)
- Sourcemapping a constructor now correctly renames the declaring type (#12)
- `this` keyword no longer gets incorrectly sourcemapped (#14)
- Code tabs now refresh when sourcemap settings are changed (#16)

## [0.2.0] - 2019-07-07
### Added
- **Managed DLL injection for debugged process**
  - Requires a method with signature `static int(string)`
  - Entrypoint and string parameter can be selected.
  - Works for .NET Framework x86
- Debug command to inject managed DLL into arbitrary process
- Debug command to show info on debugged process

## [0.1.0] - 2019-07-02
### Added
- **Add sourcemapping decompiler modes for C#, Visual Basic and IL**
  - Symbol names can be shown with different names, without modifying the binary
  - Sourcemaps can be exported and imported
- Underline managed assemblies in the treeview
- Debug command to show info on `IMenuItemContext`s

### Changed
- Targets dnSpy v6.0.5, NET472 only
