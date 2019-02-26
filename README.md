# CSharpRegexTools4Npp
Some tools for using C# Regex in Notepad++ (As Notepad++ Plugin)

Need a [Notepad++](https://notepad-plus-plus.org/) x86 or x64 installed on the machine and the right to write in the plugin directory.
(At least version 7.6.3, older versions can work but need modification of the file [NppPlugin.DllExport.targets](https://github.com/codingseb/CSharpRegexTools4Npp/blob/master/CSharpRegexTools4Npp/PluginInfrastructure/DllExport/NppPlugin.DllExport.targets) see [Notepad++ plugins new directories structures](https://notepad-plus-plus.org/community/topic/16996/new-plugins-home-round-2))

## Features
* Syntax Highlight of the C# Regex
* List all matches (With groups and captures)
* Select All matches
* Replace All matches (with replace syntax or C#)
* Extract All matches in a new Notepad++ tab
* Work on current Notepad++ tab text, on current selection or in a directory
* Named groups, lookbehind, lookforward and all features of C# Regex
* Keep an history of typed regex
* Save/Reload Regex

## Installation

* Clone this repo
* Give write access to "%PROGRAMFILES%\Notepad++\plugins\" directory
* Launch CSharpRegexTools4Npp.sln in Visual Studio
* Select the target platform x86 or x64 depending on your version of Notepad++
* Compile and launch (F5) (It will copy the plugin in the right place and launch Notepad++)

## Usage

To Launch the tools use one of these 3 methods :
* Click on the toolbar button ![ToolbarIcon](https://raw.githubusercontent.com/codingseb/CSharpRegexTools4Npp/master/CSharpRegexTools4Npp/img/icon.png)
* Click on Menu "Plugins" -> "C# Regex Tools 4 Npp" -> "C# Regex Tools"
* Press "Ctrl+Shift+H" on your keyboard

![List-Matches-Preview](https://raw.githubusercontent.com/codingseb/CSharpRegexTools4Npp/master/doc/List-Matches.gif)
![Use-Regex-Snippets](https://raw.githubusercontent.com/codingseb/CSharpRegexTools4Npp/master/doc/Use-Snippet.gif)

## Credits
Based on : [NotepadPlusPlusPluginPack](https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net) (under the [Apache-2.0 license](https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net/blob/master/LICENSE.md))  
use :  
[AvalonEdit](https://github.com/icsharpcode/AvalonEdit) For Regex edition and syntax Highlighting  (under MIT license)  
[CS-Script](https://github.com/oleg-shilo/cs-script/) For C# Match replace (under MIT license)  
[Newtonsoft.Json](https://www.newtonsoft.com/json) (under MIT license)  
[Ookii.Dialogs](http://www.ookii.org/software/dialogs/) (For Open and Save dialogs [under specific license](https://github.com/codingseb/CSharpRegexTools4Npp/blob/master/Licenses/Ooki%20license.txt))  
[PropertyChanged.Fody](https://github.com/Fody/PropertyChanged) (under MIT license)  
[Costura.fody](https://github.com/Fody/Costura) Merge all in one DLL, without it it doesn't work (under MIT license)  

And for icons :

[FatCow](https://www.fatcow.com/free-icons) (under [Creative Commons 3.0](https://creativecommons.org/licenses/by/3.0/us/))
