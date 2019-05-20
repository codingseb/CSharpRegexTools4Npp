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

public class Script
{
    //global

    public string Replace(Match match, int matchIndex, string fileName, int globalIndex, int fileIndex)
    {
        //code
    }

    public string Replace(Match match, Group group, int matchIndex, string fileName, int globalIndex, int fileIndex)
    {
        //code
    }

    public string Replace(Match match, Group group, Capture capture, int matchIndex, string fileName, int globalIndex, int fileIndex)
    {
        //code
    }

    public string Before(string text, string fileName)
    {
        //before
        return text;
        //end
    }

    public string After(string text, string fileName, List<string> fileNames = null)
    {
        //after
        return text;
        //end
    }
}