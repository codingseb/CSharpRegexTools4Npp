// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CSharpRegexTools4Npp.PluginInfrastructure
{
    public interface INotepadPPGateway
    {
        NotepadPPGateway FileNew();

        string CurrentFileName { get; }

        unsafe string GetFilePath(int bufferId);

        NotepadPPGateway SetCurrentLanguage(LangType language);
    }

    /// <summary>
    /// This class holds helpers for sending messages defined in the Msgs_h.cs file. It is at the moment
    /// incomplete. Please help fill in the blanks.
    /// </summary>
    public class NotepadPPGateway : INotepadPPGateway
    {
        public IntPtr Handle { get { return PluginBase.nppData._nppHandle; } }

        private const int Unused = 0;

        IntPtr Send(NppMsg command, int wParam, NppMenuCmd lParam)
        {
            return Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)command, wParam, lParam);
        }

        IntPtr Send(NppMsg command, int wParam, int lParam)
        {
            return Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)command, wParam, lParam);
        }

        IntPtr Send(NppMsg command, IntPtr wParam, IntPtr lParam)
        {
            return Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)command, wParam, lParam);
        }

        public NotepadPPGateway CallMenuCommand(int nppMenuCmd)
        {
            Send(NppMsg.NPPM_MENUCOMMAND, Unused, nppMenuCmd);
            return this;
        }

        public NotepadPPGateway FileNew()
        {
            Send(NppMsg.NPPM_MENUCOMMAND, Unused, NppMenuCmd.IDM_FILE_NEW);
            return this;
        }

        public NotepadPPGateway ReloadFile(string file, bool showAlert)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_RELOADFILE, showAlert ? 1 : 0, file);
            return this;
        }

        public NotepadPPGateway SaveCurrentFile()
        {
            Send(NppMsg.NPPM_SAVECURRENTFILE, Unused, Unused);
            return this;
        }

        public NotepadPPGateway SaveAllOpenedDocuments()
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SAVEALLFILES, Unused, Unused);
            return this;
        }

        public NotepadPPGateway FileExit()
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_MENUCOMMAND, Unused, NppMenuCmd.IDM_FILE_EXIT);
            return this;
        }

        public bool OpenFile(string fileName)
        {
            bool result = false;

            try
            {
                if (File.Exists(fileName))
                {
                    result = Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_DOOPEN, 0, fileName).ToInt64() == 1;
                }
            }
            catch { }

            return result;
        }

        public int GetTabIndex(string tabPath, bool smart = false)
        {
            int index = GetAllOpenedDocuments.IndexOf(tabPath);

            if (smart && index < 0)
                index = GetAllOpenedDocuments.FindIndex(tab => Path.GetFileName(tab).Equals(tabPath));

            if (smart && index < 0)
                index = GetAllOpenedDocuments.FindIndex(tab => tab.Contains(tabPath));

            return index;
        }

        public int GetTabIndex(string tabPath)
        {
            return GetTabIndex(tabPath, false);
        }

        public NotepadPPGateway ShowTab(string tabPath, bool smart)
        {
            try
            {
                ShowTab(GetTabIndex(tabPath, smart));
            }
            catch { }

            return this;
        }


        public NotepadPPGateway ShowTab(string tabPath)
        {
            try
            {
                ShowTab(GetTabIndex(tabPath));
            }
            catch { }

            return this;
        }

        public NotepadPPGateway ShowTab(int index)
        {
            try
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ACTIVATEDOC, 0, index);
            }
            catch { }

            return this;
        }

        public int TabCount
        {
            get
            {
                return Send(NppMsg.NPPM_GETNBOPENFILES, Unused, Unused).ToInt32();
            }
        }

        public IntPtr GetBufferIdFromTab(int tabIndex)
        {
            return Send(NppMsg.NPPM_GETBUFFERIDFROMPOS, tabIndex, 0);
        }

        public string GetTabFileFromPosition(int tabIndex)
        {
            var id = this.GetBufferIdFromTab(tabIndex);
            return this.GetTabFile(id);
        }

        public List<string> GetAllOpenedDocuments
        {
            get
            {
                var count = this.TabCount;

                var files = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    var file = this.GetTabFileFromPosition(i);

                    if (!string.IsNullOrEmpty(file))
                        files.Add(file);
                }
                return files;
            }
        }

        public string GetTabFile(IntPtr index)
        {
            var path = new StringBuilder(2000);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETFULLPATHFROMBUFFERID, index, path);
            return path.ToString();
        }

        /// <summary>
        /// Gets the path of plugins directory
        /// </summary>
        public string PluginsConfigDirectory
        {
            get
            {
                var path = new StringBuilder(2000);
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, path.Capacity, path);
                return path.ToString();
            }
        }

        /// <summary>
        /// Get the path of the directory where to find the executable of the current Notepad++ instance.
        /// </summary>
        public string NppBinDirectoryPath
        {
            get
            {
                var path = new StringBuilder(2000);
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETNPPDIRECTORY, path.Capacity, path);
                return path.ToString();
            }
        }

        public string NppBinVersion
        {
            get
            {
                try
                {
                    return FileVersionInfo.GetVersionInfo(Path.Combine(NppBinDirectoryPath, "notepad++.exe")).FileVersion;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the path of the current document.
        /// </summary>
        public string CurrentFileName
        {
            get
            {
                var path = new StringBuilder(2000);
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETFULLCURRENTPATH, path.Capacity, path);
                return path.ToString();
            }
        }

        /// <summary>
        /// Gets the path of the current document.
        /// </summary>
        public unsafe string GetFilePath(int bufferId)
        {
            var path = new StringBuilder(2000);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETFULLPATHFROMBUFFERID, bufferId, path);
            return path.ToString();
        }

        public NotepadPPGateway SetCurrentLanguage(LangType language)
        {
            SetCurrentLanguage((int)language);
            return this;
        }

        public unsafe NotepadPPGateway SetCurrentLanguage(string language)
        {
            fixed (byte* languagePtr = Encoding.UTF8.GetBytes(language))
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETCURRENTLANGTYPE, Unused, (IntPtr)languagePtr);
            }
            return this;
        }

        public NotepadPPGateway SetCurrentLanguage(int language)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETCURRENTLANGTYPE, Unused, language);
            return this;
        }

        public LangType GetCurrentLanguage()
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTLANGTYPE, Unused, out int language);
            return (LangType)language;
        }

        public void SetPluginMenuChecked(int id, bool check)
        {
            Send(NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[id]._cmdID, check ? 1 : 0);
        }
    }
}
