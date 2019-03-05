using CSharpRegexTools4Npp.PluginInfrastructure;
using RegexDialog;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace CSharpRegexTools4Npp
{
    
    public class Main
    {
        internal const string PluginName = "C# Regex Tools 4 Npp";
        static int idMyDlg = 0;
        static Bitmap tbBmp = Resources.icon;
        //static RegExToolDialog dialog = null;

        //Import the FindWindow API to find our window
        [DllImportAttribute("User32.dll")]
        private static extern int FindWindow(String ClassName, String WindowName);

        //Import the SetForeground API to activate it
        [DllImportAttribute("User32.dll")]
        private static extern IntPtr SetForegroundWindow(int hWnd);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int windowLongFlags, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        private enum WindowLongFlags : int
        {
            GWL_EXSTYLE = -20,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,
            DWLP_USER = 0x8,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4,
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
            toolbarIcons tbIcons = new toolbarIcons
            {
                hToolbarBmp = tbBmp.GetHbitmap()
            };

            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        public static void ShowTheDialog()
        {

            try
            {

                int hWnd = FindWindow(null, "C# Regex Tools");

                if (hWnd > 0)
                {
                    SetForegroundWindow(hWnd);
                }
                else
                {
                    var dialog = new RegExToolDialog
                    {
                        GetText = () => BNpp.Text,

                        SetText = (string text) =>
                        {
                            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                            {
                                BNpp.CreateNewDocument();
                            }

                            BNpp.Text = text;
                        },

                        SetTextInNew = (string text) =>
                        {
                            BNpp.CreateNewDocument();

                            BNpp.Text = text;
                        },

                        GetSelectedText = () => BNpp.SelectedText,

                        SetPosition = (int index, int length) => BNpp.SelectTextAndShow(index, index + length),

                        SetSelection = (int index, int length) => BNpp.AddSelection(index, index + length),

                        GetSelectionStartIndex = () => BNpp.SelectionStart,

                        GetSelectionLength = () => BNpp.SelectionLength,

                        SaveCurrentDocument = () => BNpp.SaveCurrentDocument(),

                        TryOpen = (string fileName, bool onlyIfAlreadyOpen) =>
                        {
                            try
                            {
                                bool result = false;

                                //MessageBox.Show(BNpp.CurrentPath + "\r\n" + fileName);
                                if (BNpp.CurrentPath.ToLower().Equals(fileName.ToLower()))
                                    result = true;
                                else if (BNpp.AllOpenedDocuments.Any((string s) => s.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    BNpp.ShowOpenedDocument(fileName);
                                    result = true;
                                }
                                else if (!onlyIfAlreadyOpen)
                                {
                                    result = BNpp.OpenFile(fileName);
                                }
                                else
                                {
                                    result = false;
                                }

                                hWnd = FindWindow(null, "C# Regex Tool");
                                if (hWnd > 0)
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

                        GetCurrentFileName = () => BNpp.CurrentPath
                    };

                    dialog.Show();

                    SetWindowLong(new WindowInteropHelper(dialog).Handle, (int)WindowLongFlags.GWLP_HWNDPARENT, PluginBase.nppData._nppHandle.ToInt32());
                    SetWindowLong(new WindowInteropHelper(dialog).Handle, GWL_EXSTYLE, GetWindowLong(new WindowInteropHelper(dialog).Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED);
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