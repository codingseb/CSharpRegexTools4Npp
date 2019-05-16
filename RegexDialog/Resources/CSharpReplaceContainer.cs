//using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
//using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
//using RegexDialog;

public class Script
{
    //global

    public string Replace(Match match, int index, string fileName, int globalIndex, int fileIndex)
    {
        //code
    }

    public string Replace(Match match, Group group, int index, string fileName, int globalIndex, int fileIndex)
    {
        //code
    }

    public string Replace(Match match, Group group, Capture capture, int index, string fileName, int globalIndex, int fileIndex)
    {
        //code
    }

    public string Before(string text)
    {
        //before
        return text;
        //end
    }

    public string After(string text)
    {
        //after
        return text;
        //end
    }
}