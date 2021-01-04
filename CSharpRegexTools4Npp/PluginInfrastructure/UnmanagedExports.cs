// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using CSharpRegexTools4Npp.PluginInfrastructure;
using NppPlugin.DllExport;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CSScriptNativeApi;

namespace CSharpRegexTools4Npp
{
    public class UnmanagedExports
    {
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static bool isUnicode()
        {
            return true;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void setInfo(NppData notepadPlusData)
        {
            try
            {
                PluginBase.nppData = notepadPlusData;
                Main.CommandMenuInit();
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}\r\n{exception.StackTrace}{(exception.InnerException == null ? string.Empty : $"\r\nInner Exception :\r\n{exception.InnerException.Message}\r\n{exception.InnerException.StackTrace}")}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getFuncsArray(ref int nbF)
        {
            nbF = PluginBase._funcItems.Items.Count;
            return PluginBase._funcItems.NativePointer;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        static IntPtr _ptrPluginName = IntPtr.Zero;
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getName()
        {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(Main.PluginName);
            return _ptrPluginName;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            try
            {
                ScNotification notification = (ScNotification)Marshal.PtrToStructure(notifyCode, typeof(ScNotification));
                if (notification.Header.Code == (uint)NppMsg.NPPN_TBMODIFICATION)
                {
                    PluginBase._funcItems.RefreshItems();
                    Main.SetToolBarIcon();
                }
                else if (notification.Header.Code == (uint)NppMsg.NPPN_SHUTDOWN)
                {
                    Marshal.FreeHGlobal(_ptrPluginName);
                }
                else
                {
                    Main.OnNotification(notification);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}\r\n{exception.StackTrace}{(exception.InnerException == null ? string.Empty : $"\r\nInner Exception :\r\n{exception.InnerException.Message}\r\n{exception.InnerException.StackTrace}")}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
