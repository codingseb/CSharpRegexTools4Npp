// NPP plugin platform for .Net v0.91.57 by Kasper B. Graversen etc.
using CSharpRegexTools4Npp.PluginInfrastructure;
using CSharpRegexTools4Npp.Utils;
using RegexDialog;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace CSharpRegexTools4Npp
{
    class Main
    {
        internal const int UNDO_BUFFER_SIZE = 64;
        internal const string PluginName = "C# Regex Tools 4 Npp";
        public static readonly string PluginConfigDirectory = Path.Combine(Npp.Notepad.GetConfigDirectory(), PluginName);
        public const string PluginRepository = "https://github.com/codingseb/CSharpRegexTools4Npp";
        /// <summary>
        ///  This listens to the message that Notepad++ sends when its UI language is changed.
        /// </summary>
        private static NppListener nppListener = null;
        /// <summary>
        /// If the Notepad++ version is higher than 8.6.9, this boolean does not matter.<br></br>
        /// If this is true, and the Notepad++ version is 8.6.9 or lower, <see cref="nppListener"/> will be initialized.<br></br>
        /// <b>SETTING THIS TO <c>true</c> COMES AT A REAL PERFORMANCE COST <i>EVEN WHEN YOUR PLUGIN IS NOT IN USE</i></b> (possibly up to 10% of all CPU usage associated with Notepad++)<br></br>
        /// <i>Do NOT</i> set this to true unless it is very important to you that your plugin's UI language can dynamically adjust to the Notepad++ UI language.<br></br>
        /// Note that <b>translation will work even if this is <c>false</c></b>, so the only upside of this is that
        /// the user doesn't need to close Notepad++ and restart it to see their new language preferences reflected in this plugin's UI.
        /// </summary>
        private const bool FOLLOW_NPP_UI_LANGUAGE = false;

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int LWA_ALPHA = 0x2;
        public const int LWA_COLORKEY = 0x1;

        //Import the FindWindow API to find our window
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPWStr)] string ClassName, [MarshalAs(UnmanagedType.LPWStr)] string WindowName);

        //Import the SetForeground API to activate it
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern long SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        static internal void CommandMenuInit()
        {
            // first make it so that all references to any third-party dependencies point to the correct location
            // see https://github.com/oleg-shilo/cs-script.npp/issues/66#issuecomment-1086657272 for more info
            AppDomain.CurrentDomain.AssemblyResolve += LoadDependency;

            PluginBase.SetCommand(0, "C# Regex Tools", ShowTheDialog, new ShortcutKey(true, false, true, System.Windows.Forms.Keys.H));
            PluginBase.SetCommand(1, "&Documentation", Docs);

            
            if (FOLLOW_NPP_UI_LANGUAGE && (Npp.nppVersion[0] < 8 || (Npp.nppVersion[0] == 8 && (Npp.nppVersion[1] < 6 || (Npp.nppVersion[1] == 6 && Npp.nppVersion[2] <= 9)))))
            {
                // start listening to messages that aren't broadcast by the plugin manager (for versions of Notepad++ 8.6.9 or earlier, because later versions have NPPN_NATIVELANGCHANGED)
                nppListener = new NppListener();
                nppListener.AssignHandle(PluginBase.nppData._nppHandle);
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

                    //RegExToolDialog.InitIsOK = () => CheckUpdates(dialog);

                    dialog = new RegExToolDialog
                    {
                        GetText = () => Npp.Editor.GetText(),

                        SetText = text =>
                        {
                            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                            {
                                Npp.Notepad.FileNew();
                            }

                            Npp.Editor.SetText(text);
                        },

                        SetTextInNew = text =>
                        {
                            Npp.Notepad.FileNew();

                            Npp.Editor.SetText(text);
                        },

                        GetSelectedText = () => "",

                        SetPosition = (index, length) =>
                        {
                            Npp.Editor.SetSelectionStart(index);
                            Npp.Editor.SetSelectionEnd(index + length);
                        },

                        SetSelection = (index, length) => Npp.Editor.AddSelection(index, index + length),

                        GetSelectionStartIndex = () => Npp.Editor.GetSelectionStart(),

                        GetSelectionLength = () => Npp.Editor.GetSelectionLength(),

                        SaveCurrentDocument = () => Npp.Notepad.SaveCurrentFile(),

                        SetCurrentTabInCSharpHighlighting = () => Npp.Notepad.SetCurrentLanguage(LangType.L_CS),

                        TryOpen = (fileName, onlyIfAlreadyOpen) =>
                        {
                            try
                            {
                                bool result = false;

                                if (Npp.Notepad.CurrentFileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                                {
                                    result = true;
                                }
                                else if (Npp.Notepad.GetAllOpenedDocuments.Any(s => s.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    Npp.Notepad.ShowTab(fileName);
                                    result = true;
                                }
                                else if (!onlyIfAlreadyOpen)
                                {
                                    result = Npp.Notepad.OpenFile(fileName);
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

                        GetCurrentFileName = () => Npp.Notepad.CurrentFileName
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

        private static Assembly LoadDependency(object sender, ResolveEventArgs args)
        {
            string assemblyFile = Path.Combine(Npp.pluginDllDirectory, new AssemblyName(args.Name).Name) + ".dll";
            if (File.Exists(assemblyFile))
                return Assembly.LoadFrom(assemblyFile);
            return null;
        }

        public static void OnNotification(ScNotification notification)
        {
            
        }

        static internal void PluginCleanUp()
        {
        }

        /// <summary>
        /// open GitHub repo with the web browser
        /// </summary>
        private static void Docs()
        {
            OpenUrlInWebBrowser(PluginRepository);
        }

        public static void OpenUrlInWebBrowser(string url)
        {
            try
            {
                var ps = new ProcessStartInfo(url)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(ps);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"While attempting to open URL {url} in web browser, got exception\r\n{ex}",
                    "Could not open url in web browser",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}   
