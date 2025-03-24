// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Interop;
using CSharpRegexTools4Npp.PluginInfrastructure;
using CSharpRegexTools4Npp.Utils;

namespace CSharpRegexTools4Npp.PluginInfrastructure
{
    /// <summary>
    /// This class holds helpers for sending messages defined in the Msgs_h.cs file. It is at the moment
    /// incomplete. Please help fill in the blanks.
    /// </summary>
    public class NotepadPPGateway
	{
        public IntPtr Handle { get { return PluginBase.nppData._nppHandle; } }

        private const int Unused = 0;

        IntPtr Send(NppMsg command, int wParam, NppMenuCmd lParam)
        {
            return Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)command, wParam, lParam);
        }

        private IntPtr Send(NppMsg command, int wParam, int lParam)
        {
            return Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)command, wParam, lParam);
        }

        private IntPtr Send(NppMsg command, IntPtr wParam, IntPtr lParam)
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


        public void AddToolbarIcon(int funcItemsIndex, ToolbarIcons icon)
		{
			IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(icon));
			try {
				Marshal.StructureToPtr(icon, pTbIcons, false);
				_ = Win32.SendMessage(
					PluginBase.nppData._nppHandle,
					(uint) NppMsg.NPPM_ADDTOOLBARICON,
					PluginBase._funcItems.Items[funcItemsIndex]._cmdID,
					pTbIcons);
			} finally {
				Marshal.FreeHGlobal(pTbIcons);
			}
		}

		public void AddToolbarIcon(int funcItemsIndex, Bitmap icon)
		{
            ToolbarIcons tbi = new ToolbarIcons
            {
                hToolbarBmp = icon.GetHbitmap()
            };
            AddToolbarIcon(funcItemsIndex, tbi);
		}

		/// <summary>
		/// Gets the path of the current document.
		/// </summary>
		public string GetCurrentFilePath()
		{
			var path = new StringBuilder(2000);
			Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETFULLCURRENTPATH, 0, path);
			return path.ToString();
		}

		/// <summary>
		/// This method incapsulates a common pattern in the Notepad++ API: when
		/// you need to retrieve a string, you can first query the buffer size.
		/// This method queries the necessary buffer size, allocates the temporary
		/// memory, then returns the string retrieved through that buffer.
		/// </summary>
		/// <param name="message">Message ID of the data string to query.</param>
		/// <returns>String returned by Notepad++.</returns>
		public string GetString(NppMsg message)
		{
			int len = Win32.SendMessage(
					PluginBase.nppData._nppHandle,
					(uint) message, Unused, Unused).ToInt32()
				+ 1;
			var res = new StringBuilder(len);
			_ = Win32.SendMessage(
				PluginBase.nppData._nppHandle, (uint) message, len, res);
			return res.ToString();
		}

		/// <returns>The path to the Notepad++ executable.</returns>
		public string GetNppPath()
			=> GetString(NppMsg.NPPM_GETNPPDIRECTORY);

		/// <returns>The path to the Config folder for plugins.</returns>
		public string GetPluginConfigPath()
			=> GetString(NppMsg.NPPM_GETPLUGINSCONFIGDIR);

		/// <summary>
		/// Gets the path of the current document.
		/// </summary>
		public unsafe string GetFilePath(IntPtr bufferId)
		{
			var path = new StringBuilder(2000);
			Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_GETFULLPATHFROMBUFFERID, bufferId, path);
			return path.ToString();
		}

		public void HideDockingForm(System.Windows.Forms.Form form)
		{
			Win32.SendMessage(PluginBase.nppData._nppHandle,
					(uint)(NppMsg.NPPM_DMMHIDE),
					0, form.Handle);
		}

		public void ShowDockingForm(System.Windows.Forms.Form form)
		{
			Win32.SendMessage(PluginBase.nppData._nppHandle,
					(uint)(NppMsg.NPPM_DMMSHOW),
					0, form.Handle);
		}

		public void ShowDockingForm(System.Windows.Window window)
		{
			Win32.SendMessage(PluginBase.nppData._nppHandle,
					(uint)(NppMsg.NPPM_DMMSHOW),
					0, new WindowInteropHelper(window).Handle);
		}

		public Color GetDefaultForegroundColor()
		{
			var rawColor = (int)Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETEDITORDEFAULTFOREGROUNDCOLOR, 0, 0);
			return Color.FromArgb(rawColor & 0xff, (rawColor >> 8) & 0xff, (rawColor >> 16) & 0xff);
		}

		public Color GetDefaultBackgroundColor()
		{
			var rawColor = (int)Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETEDITORDEFAULTBACKGROUNDCOLOR, 0, 0);
			return Color.FromArgb(rawColor & 0xff, (rawColor >> 8) & 0xff, (rawColor >> 16) & 0xff);
		}

		/// <summary>
		/// Figure out default N++ config file path<br></br>
		/// Path is usually -> .\Users\<username>\AppData\Roaming\Notepad++\plugins\config\
		/// </summary>
		public string GetConfigDirectory()
        {
			var sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
			Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
			return sbIniFilePath.ToString();
		}

		/// <summary>
		/// 3-int array: {major, minor, bugfix}<br></br>
		/// Thus GetNppVersion() would return {8, 5, 0} for version 8.5.0
		/// and {7, 7, 1} for version 7.7.1
		/// </summary>
		/// <returns></returns>
		public int[] GetNppVersion()
		{
			int version = Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETNPPVERSION, 0, 0).ToInt32();
			int major = version >> 16;
			int minor = Math.DivRem(version & 0xffff, 10, out int bugfix);
			if (minor == 0)
				(bugfix, minor) = (minor, bugfix);
			return new int[] { major, minor, bugfix };
        }

        /// <summary>
		/// Get all open filenames in both views (all in first view, then all in second view)
		/// </summary>
		/// <returns></returns>
        public string[] GetOpenFileNames()
        {
            var bufs = new List<string>();
            foreach (int view in GetVisibleViews())
            {
                int nbOpenFiles = (int)Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETNBOPENFILES, 0, view + 1);
                for (int ii = 0; ii < nbOpenFiles; ii++)
                {
                    IntPtr bufId = Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETBUFFERIDFROMPOS, ii, view);
                    bufs.Add(Npp.Notepad.GetFilePath(bufId));
                }
            }
            return bufs.ToArray();
        }

        public void AddModelessDialog(IntPtr formHandle)
		{
			Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_MODELESSDIALOG, IntPtr.Zero, formHandle);
		}

        public void RemoveModelessDialog(IntPtr formHandle)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_MODELESSDIALOG, new IntPtr(1), formHandle);
        }

		/// <summary>
		/// the status bar is the bar at the bottom with the document type, EOL type, current position, line, etc.<br></br>
		/// Set the message for one of the sections of that bar.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="section"></param>
		public void SetStatusBarSection(string message, StatusBarSection section)
		{
			Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_SETSTATUSBAR, (int)section, message);
		}

        public unsafe bool AllocateIndicators(int numberOfIndicators, out int[] indicators)
		{
			indicators = null;
			if (numberOfIndicators < 1)
				return false;
			indicators = new int[numberOfIndicators];
			fixed (int * indicatorsPtr = indicators)
			{
				IntPtr success = Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ALLOCATEINDICATOR, (IntPtr)numberOfIndicators, (IntPtr)indicatorsPtr);
				for (int ii = 1; ii < numberOfIndicators; ii++)
					indicators[ii] = indicators[ii - 1] + 1;
				return success != IntPtr.Zero;
			}
		}

		public unsafe bool TryGetNativeLangName(out string langName)
		{
			langName = "";
			int fnameLen = Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETNATIVELANGFILENAME, IntPtr.Zero, IntPtr.Zero).ToInt32() + 1;
			if (fnameLen == 1)
				return false;
			var fnameArr = new byte[fnameLen];
			fixed (byte * fnameBuf = fnameArr)
			{
				IntPtr fnamePtr = (IntPtr)fnameBuf;
				Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETNATIVELANGFILENAME, (IntPtr)fnameLen, fnamePtr);
				langName = Marshal.PtrToStringAnsi(fnamePtr);
            }
			if (!string.IsNullOrEmpty(langName) && langName.EndsWith(".xml"))
			{
				langName = langName.Substring(0, langName.Length - 4);
				return true;
			}
			return false;
		}

		public List<int> GetVisibleViews()
		{
			var openViews = new List<int>();
			for (int view = 0; view < 2; view++)
			{
				// NPPM_GETCURRENTDOCINDEX(0, view) returns -1 if that view is invisible
				if ((int)Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTDOCINDEX, 0, view) >= 0)
					openViews.Add(view);
			}
			return openViews;
		}
    }

	/// <summary>
	/// This class holds helpers for sending messages defined in the Resource_h.cs file. It is at the moment
	/// incomplete. Please help fill in the blanks.
	/// </summary>
	class NppResource
	{
		private const int Unused = 0;

		public void ClearIndicator()
		{
			Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) Resource.NPPM_INTERNAL_CLEARINDICATOR, Unused, Unused);
		}
	}
}
