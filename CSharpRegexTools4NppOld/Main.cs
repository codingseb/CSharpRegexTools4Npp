using CSharpRegexTools4Npp.PluginInfrastructure;
using Newtonsoft.Json.Linq;
using RegexDialog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace CSharpRegexTools4Npp
{
    public static class Main
    {
        internal const string PluginName = "C# Regex Tools 4 Npp";
        private static int idMyDlg = 0;
        private static readonly Bitmap tbBmp = Resources.icon;
        //static RegExToolDialog dialog = null;

        //Import the FindWindow API to find our window
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPWStr)] string ClassName, [MarshalAs(UnmanagedType.LPWStr)] string WindowName);

        //Import the SetForeground API to activate it
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int windowLongFlags, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern long SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        private enum WindowLongFlags : int
        {
            GWL_USERDATA = -21,
            GWL_EXSTYLE = -20,
            GWL_STYLE = -16,
            GWL_ID = -12,
            GWLP_HWNDPARENT = -8,
            GWLP_HINSTANCE = -6,
            GWL_WNDPROC = -4,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4,
            DWLP_USER = 0x8,
            WS_EX_LAYERED = 0x80000
        }

        private enum LayeredWindowAttributesFlags : byte
        {
            LWA_COLORKEY = 0x1,
            LWA_ALPHA = 0x2
        }

        public static void OnNotification(ScNotification notification)
        {
        }

        internal static void CommandMenuInit()
        {
            PluginBase.SetCommand(0, "C# Regex Tools", ShowTheDialog, new ShortcutKey(true, false, true, System.Windows.Forms.Keys.H));
            idMyDlg = 0;
        }

        internal static void SetToolBarIcon()
        {
            if (!string.IsNullOrEmpty(Npp.NotepadPP.NppBinVersion)
                && int.TryParse(Npp.NotepadPP.NppBinVersion.Split('.')[0], out int majorVersion)
                && majorVersion >= 8)
            {
                toolbarIconsWithDarkMode tbIcons = new()
                {
                    hToolbarBmp = tbBmp.GetHbitmap(),
                    hToolbarIcon = tbBmp.GetHicon(),
                    hToolbarIconDarkMode = tbBmp.GetHicon()
                };

                IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
                Marshal.StructureToPtr(tbIcons, pTbIcons, false);
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON_FORDARKMODE, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);
                Marshal.FreeHGlobal(pTbIcons);
            }
            else
            {
                toolbarIcons tbIcons = new()
                {
                    hToolbarBmp = tbBmp.GetHbitmap(),
                    hToolbarIcon = tbBmp.GetHicon(),
                };

                IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
                Marshal.StructureToPtr(tbIcons, pTbIcons, false);
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);
                Marshal.FreeHGlobal(pTbIcons);
            }
        }

        private static async void CheckUpdates(RegExToolDialog dialog)
        {
            double hoursFromLastCheck = Math.Abs((DateTime.Now - Config.Instance.LastUpdateCheck).TotalHours);

            //if (hoursFromLastCheck > 8)
            if (true)
            {
                ServicePointManager.ServerCertificateValidationCallback += (_, __, ___, ____) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                try
                {
                    HttpClient client = new();
                    client.DefaultRequestHeaders.Add("Accept-Language", "en-US;q=0.5,en;q=0.3");
                    client.DefaultRequestHeaders.Add("User-Agent", "C# App");

                    var response = await client.GetAsync("https://api.github.com/repos/codingseb/CSharpRegexTools4Npp/releases/latest").ConfigureAwait(true);

                    string responseText = await response.Content.ReadAsStringAsync();

                    var jsonResult = JObject.Parse(responseText);

                    int[] latestVersion = jsonResult["name"].ToString().Split('.').Select(digit => int.Parse(digit.Trim())).ToArray();
                    int[] currentVersion = typeof(RegExToolDialog).Assembly.GetName().Version.ToString().Split('.').Select(digit => int.Parse(digit.Trim())).ToArray();

                    Debug.WriteLine($"{latestVersion} - {currentVersion}");

                    for(int i = 0; i < latestVersion.Length && i < currentVersion.Length;i++)
                    {
                        if(latestVersion[i] > currentVersion[i])
                        {
                            Config.Instance.UpdateAvailable = true;
                            Config.Instance.UpdateURL = "https://github.com/codingseb/CSharpRegexTools4Npp/releases";
                            break;
                        }
                        else if(latestVersion[i] < currentVersion[i])
                        {
                            break;
                        }
                    }
                }
                catch
                { }
            }
        }

        public static void ShowTheDialog()
        {
            try
            {
                AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = @"plugins\CSharpRegexTools4Npp";

                IntPtr hWnd = FindWindow(null, $"C# Regex Tools - {Assembly.GetExecutingAssembly().GetName().Version}");

                if (hWnd.ToInt64() > 0)
                {
                    SetForegroundWindow(hWnd);
                }
                else
                {
                    RegExToolDialog dialog = null;

                    RegExToolDialog.InitIsOK = () => CheckUpdates(dialog);

                    dialog = new RegExToolDialog
                    {
                        GetText = () => Npp.Text,

                        SetText = text =>
                        {
                            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                            {
                                Npp.NotepadPP.FileNew();
                            }

                            Npp.Text = text;
                        },

                        SetTextInNew = text =>
                        {
                            Npp.NotepadPP.FileNew();

                            Npp.Text = text;
                        },

                        GetSelectedText = () => Npp.SelectedText,

                        SetPosition = (index, length) => Npp.SelectTextAndShow(index, index + length),

                        SetSelection = (index, length) => Npp.AddSelection(index, index + length),

                        GetSelectionStartIndex = () => Npp.SelectionStart,

                        GetSelectionLength = () => Npp.SelectionLength,

                        SaveCurrentDocument = () => Npp.NotepadPP.SaveCurrentFile(),

                        SetCurrentTabInCSharpHighlighting = () => Npp.NotepadPP.SetCurrentLanguage(LangType.L_CS),

                        TryOpen = (fileName, onlyIfAlreadyOpen) =>
                        {
                            try
                            {
                                bool result = false;

                                if (Npp.NotepadPP.CurrentFileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                                {
                                    result = true;
                                }
                                else if (Npp.NotepadPP.GetAllOpenedDocuments.Any(s => s.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    Npp.NotepadPP.ShowTab(fileName);
                                    result = true;
                                }
                                else if (!onlyIfAlreadyOpen)
                                {
                                    result = Npp.NotepadPP.OpenFile(fileName);
                                }
                                else
                                {
                                    result = false;
                                }

                                hWnd = FindWindow(null, $"C# Regex Tool - {Assembly.GetExecutingAssembly().GetName().Version}");
                                if (hWnd.ToInt64() > 0)
                                {
                                    SetForegroundWindow(hWnd);
                                }

                                return result;
                            }
                            catch
                            {
                                return false;
                            }
                        },

                        GetCurrentFileName = () => Npp.NotepadPP.CurrentFileName
                    };

                    dialog.Show();

                    Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.MODELESSDIALOGADD, (int)NppMsg.NPPM_MODELESSDIALOG, new WindowInteropHelper(dialog).Handle.ToInt32());
                    //SetWindowLong(new WindowInteropHelper(dialog).Handle, (int)WindowLongFlags.GWLP_HWNDPARENT, PluginBase.nppData._nppHandle);
                    SetLayeredWindowAttributes(new WindowInteropHelper(dialog).Handle, 0, 128, LWA_ALPHA);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + "\r\n" + exception.StackTrace, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}