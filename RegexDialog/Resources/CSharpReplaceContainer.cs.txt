using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RegexDialog;
//usings

public class CSharpReplaceContainer
{
    //global

    //match
    public string Replace(Match match, int matchIndex, string fileName, int globalIndex, int fileIndex)
    {
        //code
    }
    //endmatch

    //group
    public string Replace(Match match, Group group, int matchIndex, string fileName, int globalIndex, int fileIndex)
    {
        //code
    }
    //endgroup

    //capture
    public string Replace(Match match, Group group, Capture capture, int matchIndex, string fileName, int globalIndex, int fileIndex)
    {
        //code
    }
    //endcapture

    public string Before(string text, string fileName)
    {
        //before
    }

    public string After(string text, string fileName, List<string> fileNames = null)
    {
        //after
    }
}