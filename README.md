# CSharpRegexTools4Npp
Some tools for using C# Regex in Notepad++ (As Notepad++ Plugin)

Need a [Notepad++](https://notepad-plus-plus.org/) x86 or x64 installed on the machine and the right to write in the plugin directory.
(At least version 7.6.3, older versions can work but need modification of the file [NppPlugin.DllExport.targets](https://github.com/codingseb/CSharpRegexTools4Npp/blob/master/CSharpRegexTools4Npp/PluginInfrastructure/DllExport/NppPlugin.DllExport.targets))

## Installation

* Clone this repo
* Give write access to "%NOTEPAD++Root%\plugins" directory
* Launch CSharpRegexTools4Npp.sln in Visual Studio
* Select the target platform x86 or x64 depending on your version of Notepad++
* Compile and launch (F5) (It will copy the plugin in the right place and launch Notepad++)

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
