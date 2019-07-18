Changelog
=========

## v0.4.0
Other changes:
- Namespace and Type are now stored seperately in config files. Old config files will no longer contain valid recent injections.

## v0.3.0
Features:
- Reinject a recently injected DLL
- Export/import sourcemaps
- Add setting for copying injected DLLs to temporary directory before injecting
- Add setting for automatically renaming DLLImports if they don't have a mapped name already
- Add setting for automatically renaming overridden members

Bugfixes:
- "this" keyword is no longer sourcemapped
- Mapping constructors will map their declaring type

## v0.2.0
- DLL injection in debugged .NET Framework processes

## v0.1.0
- Rename symbols in decompiler without modifying binary (sourcemapping)
- Underline .NET assemblies in tree view
