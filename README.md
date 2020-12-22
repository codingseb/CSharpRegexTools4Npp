# CSharpRegexTools4Npp
Some tools to use C# Regex in Notepad++ (As Notepad++ Plugin)

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

## Prerequistes
Need a [Notepad++](https://notepad-plus-plus.org/) x86 or x64 installed on the machine and the right to write in the plugin directory.
(At least version 7.6.3, older versions can work but need modification of the file [NppPlugin.DllExport.targets](https://github.com/codingseb/CSharpRegexTools4Npp/blob/master/CSharpRegexTools4Npp/PluginInfrastructure/DllExport/NppPlugin.DllExport.targets) see [Notepad++ plugins new directories structures](https://notepad-plus-plus.org/community/topic/16996/new-plugins-home-round-2))

Need .Net Framework 4.7 or greater

## Installation

* Download the [last release zip](https://github.com/codingseb/CSharpRegexTools4Npp/releases)
* Uncompress it in the "%PROGRAMFILES%\Notepad++\plugins\" directory

**Or**

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

#### Show matches of the Regex
![List-Matches-Preview](https://raw.githubusercontent.com/codingseb/CSharpRegexTools4Npp/master/doc/List-Matches.gif)
#### Use Regex snippets
![Use-Regex-Snippets](https://raw.githubusercontent.com/codingseb/CSharpRegexTools4Npp/master/doc/Use-Snippet.gif)
#### Replace all occurences
![Replace-all-](https://raw.githubusercontent.com/codingseb/CSharpRegexTools4Npp/master/doc/Replace-All.gif)
#### Replace with C# script
![Replace-With-C#](https://raw.githubusercontent.com/codingseb/CSharpRegexTools4Npp/master/doc/Replace-with-CSharp.gif)
#### Standards Regex operations from the toolbar
![Toolbar-Actions](https://raw.githubusercontent.com/codingseb/CSharpRegexTools4Npp/master/doc/Standard-Operations.png)
1. **Is Match ?** : Show a "Yes" if at least one match is found in the text source. Show a "No" otherwise
2. **Matches** : Fill the list of all found matches with Groups and Captures
3. **Select All Matches** : Select all found matches in the current Notepad++ tab (Can be use for multi-edition)
4. **Extract All** : Extract all matches results in a new Notepad++ tab
5. **Replace All** : Replace all matches in the text source
#### Some others functionalities (options) preview
![Replace-Elements](https://raw.githubusercontent.com/codingseb/CSharpRegexTools4Npp/master/doc/Replace-elements.png)  
  
![Replace-Elements](https://raw.githubusercontent.com/codingseb/CSharpRegexTools4Npp/master/doc/Regex-Options.png) 
  
![Text-sources](https://raw.githubusercontent.com/codingseb/CSharpRegexTools4Npp/master/doc/Text-Sources.png) 

## Credits
Based on : [NotepadPlusPlusPluginPack](https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net) ([Apache-2.0 license](https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net/blob/master/LICENSE.md)) Merged manually with a part of the version used in [CS-Script.Npp](https://github.com/oleg-shilo/cs-script.npp) (under MIT license) for better x64 support and remodified for the way I use it.  
use :  
[AvalonEdit](https://github.com/icsharpcode/AvalonEdit) For Regex edition and syntax Highlighting  (MIT license)  
[CS-Script](https://github.com/oleg-shilo/cs-script/) For C# Match replace (MIT license)  
[Newtonsoft.Json](https://www.newtonsoft.com/json) (MIT license)  
[Ookii.Dialogs](http://www.ookii.org/software/dialogs/) (For Open and Save dialogs [specific license](https://github.com/codingseb/CSharpRegexTools4Npp/blob/master/Licenses/Ooki%20license.txt))  
[PropertyChanged.Fody](https://github.com/Fody/PropertyChanged) (MIT license)  
[Costura.fody](https://github.com/Fody/Costura) Merge all in one DLL, without it it doesn't work (MIT license)  

And for icons :

[FatCow](https://www.fatcow.com/free-icons) ([Creative Commons 3.0](https://creativecommons.org/licenses/by/3.0/us/))
