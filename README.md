# Msiler: CIL (MSIL) Code Viewer

CIL (MSIL) code viewer extension for Visual Studio 2017.

[![Build status](https://ci.appveyor.com/api/projects/status/aj46np0rwam5sf4e?svg=true)](https://ci.appveyor.com/project/segrived/msiler) [![segrived MyGet Build Status](https://www.myget.org/BuildSource/Badge/segrived?identifier=096d8b99-21b2-47b6-8d8a-62b7b7bf5fb9)](https://www.myget.org/)

## Screenshot

![Screenshot](http://i.imgur.com/itDM13A.gif)

## Installation
You can install extension from [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/60fc53d4-e414-461b-a27c-3d5d2a53f637) or download from [releases section](https://github.com/segrived/Msiler/releases) and install manually.

[**Last build with Visual Studio 2012/2013/2015 support**](https://ci.appveyor.com/project/segrived/msiler/build/1.0.213/artifacts)

[Get latest dev version](https://ci.appveyor.com/project/segrived/msiler/build/artifacts) (can be unstable)

## Usage
Open tool window using menu item "Tools ▶ Msiler: MSIL Code Viewer" and build/rebuild project.

## Options
### Global options

* **Update listing only if toolbox is visible** - if enabled, bytecode listing will be updated, only if Msiler windows is visible. It can be useful, because hidden Msiler window will not perform any actions. This feature should be disabled, if you has Msiler as background tab (for example), because in this case bytecode content will not be updated. (default: true)

### User interface
* **Font name**: Bytecode listing font name *(default: Consolas)*
* **Font size**: Bytecode listing font size *(default: 12)*
* **Show line numbers**: if enabled line numbers will be displayed *(default: true)*
* **VS Color theme**: Visual studio color theme, bytecode listing highlighting will be adjusted based on this value *(default: Auto)*

### Excluded methods

Some methods now can be excluded from method list:

* **Exclude getters/setters** - if true, generated getters/setters (methods started with ```get_``` or ```set_```) will be excluded from method list *(default: false)*
* **Exclude anonymous methods** - if true, anonymous methods (methods contains ```<``` and ```>``` characters in names) will be excluded from method list *(default: false)*
* **Exclude constructors** - if true, contructors (.ctor methods) will be excluded from method list *(default: false)*

### Listing generation options

* **Ignore NOPs** - if true, extension will ignore all NOP instructions (0x00 bytes) *(default: false)*
* **Display numbers as HEX values** - if true, numbers in listing will be displayed as hex values (for example 123 will be displayed as 0x7B) *(default: false)*
* **Display offsets as decimal numbers** - if true, offsets in listing will be displayed as decimal numbers instead of HEX representation (IL_0427 will be shown as IL_01AB) *(default: false)*
* **Simplify function names** - if true, function names will be simplified (experimental) *(default: false)*
* **Upcase OpCodes** - if true, all OpCode names will be upcased (ldarg.0 will be displayed as LDARG.0) *(default: false)*
* **Align listing** - if true, all instructions in listing will be aliged *(default: false)*

Example: With disabled option

```
IL_0010 brtrue.s IL_0026
IL_0012 call WTManager.ConfigManager.get_Preferences
IL_0017 callvirt WTManager.Preferences.get_EditorPath
IL_001C call System.IO.File.Exists
IL_0021 ldc.i4.0
```

With enabled option
```
IL_0010 brtrue.s   IL_0026
IL_0012 call       WTManager.ConfigManager.get_Preferences
IL_0017 callvirt   WTManager.Preferences.get_EditorPath
IL_001C call       System.IO.File.Exists
IL_0021 ldc.i4.0
```

## Pull requests
Feel free to send pull requests.

## Changelog
[Version history](https://github.com/segrived/Msiler/wiki/Version-History)

## Roadmap

### Version 2.1 (Feb 2016)
* PDB files support
* Follow methods under cursor (optional)
* Additional method information window

### Version 2.2 (Mar 2016)
* Clickable offsets
* Show anonymous function with a method that invoked it (optional)

## License
The MIT License (MIT)

Copyright (c) 2015 segrived

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Credits
* Extension icon: [Puzzle by SooAnne](https://thenounproject.com/term/puzzle/23932/) from the [Noun Project](https://thenounproject.com/)
* [dnlib](https://github.com/0xd4d/dnlib) by [0xd4d](https://github.com/0xd4d)
