﻿// NPP plugin platform for .Net v0.94.00 by Kasper B. Graversen etc.
using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace CSharpRegexTools4Npp.PluginInfrastructure
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NppData
    {
        public IntPtr _nppHandle;
        public IntPtr _scintillaMainHandle;
        public IntPtr _scintillaSecondHandle;
    }

    public delegate void NppFuncItemDelegate();

    [StructLayout(LayoutKind.Sequential)]
    public struct ShortcutKey
    {
        public ShortcutKey(string data)
        {
            //Ctrl+Shift+Alt+Key
            var parts = data.Split('+');
            _key = Convert.ToByte(Enum.Parse(typeof(Keys), parts.Last()));
            parts = parts.Take(parts.Length - 1).ToArray();
            _isCtrl = Convert.ToByte(parts.Contains("Ctrl"));
            _isShift = Convert.ToByte(parts.Contains("Shift"));
            _isAlt = Convert.ToByte(parts.Contains("Alt"));
        }

        public ShortcutKey(bool isCtrl, bool isAlt, bool isShift, Keys key)
        {
            // the types 'bool' and 'char' have a size of 1 byte only!
            _isCtrl = Convert.ToByte(isCtrl);
            _isAlt = Convert.ToByte(isAlt);
            _isShift = Convert.ToByte(isShift);
            _key = Convert.ToByte(key);
        }

        public bool IsCtrl { get { return _isCtrl != 0; } }
        public bool IsShift { get { return _isShift != 0; } }
        public bool IsAlt { get { return _isAlt != 0; } }
        public Keys Key { get { return (Keys)_key; } }
        public string AsText { get => Key == Keys.None ? null : (IsCtrl ? "Ctrl+" : string.Empty) + (IsAlt ? "Alt+" : string.Empty) + (IsShift ? "Shift+" : string.Empty) + Key.ToString(); }

        public override string ToString()
        {
            return AsText;
        }

        public byte _isCtrl;
        public byte _isAlt;
        public byte _isShift;
        public byte _key;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FuncItem
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string _itemName;
        public NppFuncItemDelegate _pFunc;
        public int _cmdID;
        public bool _init2Check;
        public ShortcutKey _pShKey;
    }

    public class FuncItems : IDisposable
    {
        List<FuncItem> _funcItems;
        int _sizeFuncItem;
        List<IntPtr> _shortCutKeys;
        IntPtr _nativePointer;
        bool _disposed = false;

        public FuncItems()
        {
            _funcItems = new List<FuncItem>();
            _sizeFuncItem = Marshal.SizeOf(typeof(FuncItem));
            _shortCutKeys = new List<IntPtr>();
        }

        [DllImport("kernel32")]
        static extern void RtlMoveMemory(IntPtr Destination, IntPtr Source, int Length);
        public void Add(FuncItem funcItem)
        {
            int oldSize = _funcItems.Count * _sizeFuncItem;
            _funcItems.Add(funcItem);
            int newSize = _funcItems.Count * _sizeFuncItem;
            IntPtr newPointer = Marshal.AllocHGlobal(newSize);

            if (_nativePointer != IntPtr.Zero)
            {
                RtlMoveMemory(newPointer, _nativePointer, oldSize);
                Marshal.FreeHGlobal(_nativePointer);
            }
            IntPtr ptrPosNewItem = (IntPtr)(newPointer.ToInt64() + oldSize);
            byte[] aB = Encoding.Unicode.GetBytes(funcItem._itemName + "\0");
            Marshal.Copy(aB, 0, ptrPosNewItem, aB.Length);
            ptrPosNewItem = (IntPtr)(ptrPosNewItem.ToInt64() + 128);
            IntPtr p = (funcItem._pFunc != null) ? Marshal.GetFunctionPointerForDelegate(funcItem._pFunc) : IntPtr.Zero;
            Marshal.WriteIntPtr(ptrPosNewItem, p);
            ptrPosNewItem = (IntPtr)(ptrPosNewItem.ToInt64() + IntPtr.Size);
            Marshal.WriteInt32(ptrPosNewItem, funcItem._cmdID);
            ptrPosNewItem = (IntPtr)(ptrPosNewItem.ToInt64() + 4);
            Marshal.WriteInt32(ptrPosNewItem, Convert.ToInt32(funcItem._init2Check));
            ptrPosNewItem = (IntPtr)(ptrPosNewItem.ToInt64() + 4);
            if (funcItem._pShKey._key != 0)
            {
                IntPtr newShortCutKey = Marshal.AllocHGlobal(4);
                Marshal.StructureToPtr(funcItem._pShKey, newShortCutKey, false);
                Marshal.WriteIntPtr(ptrPosNewItem, newShortCutKey);
            }
            else Marshal.WriteIntPtr(ptrPosNewItem, IntPtr.Zero);

            _nativePointer = newPointer;
        }

        public void RefreshItems()
        {
            IntPtr ptrPosItem = _nativePointer;
            for (int i = 0; i < _funcItems.Count; i++)
            {
                FuncItem updatedItem = new();
                updatedItem._itemName = _funcItems[i]._itemName;
                ptrPosItem = (IntPtr)(ptrPosItem.ToInt64() + 128);
                updatedItem._pFunc = _funcItems[i]._pFunc;
                ptrPosItem = (IntPtr)(ptrPosItem.ToInt64() + IntPtr.Size);
                updatedItem._cmdID = Marshal.ReadInt32(ptrPosItem);
                ptrPosItem = (IntPtr)(ptrPosItem.ToInt64() + 4);
                updatedItem._init2Check = _funcItems[i]._init2Check;
                ptrPosItem = (IntPtr)(ptrPosItem.ToInt64() + 4);
                updatedItem._pShKey = _funcItems[i]._pShKey;
                ptrPosItem = (IntPtr)(ptrPosItem.ToInt64() + IntPtr.Size);

                _funcItems[i] = updatedItem;
            }
        }

        public IntPtr NativePointer { get { return _nativePointer; } }
        public List<FuncItem> Items { get { return _funcItems; } }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                try
                {
                    foreach (IntPtr ptr in _shortCutKeys) Marshal.FreeHGlobal(ptr);
                    if (_nativePointer != IntPtr.Zero) Marshal.FreeHGlobal(_nativePointer);
                }
                catch { }
            }
        }
        ~FuncItems()
        {
            Dispose();
        }
    }


    public enum winVer
    {
        WV_UNKNOWN, WV_WIN32S, WV_95, WV_98, WV_ME, WV_NT, WV_W2K,
        WV_XP, WV_S2003, WV_XPX64, WV_VISTA, WV_WIN7, WV_WIN8, WV_WIN81, WV_WIN10
    }


    [Flags]
    public enum DockMgrMsg : uint
    {
        IDB_CLOSE_DOWN = 137,
        IDB_CLOSE_UP = 138,
        IDD_CONTAINER_DLG = 139,

        IDC_TAB_CONT = 1027,
        IDC_CLIENT_TAB = 1028,
        IDC_BTN_CAPTION = 1050,

        DMM_MSG = 0x5000,
        DMM_CLOSE = DMM_MSG + 1,
        DMM_DOCK = DMM_MSG + 2,
        DMM_FLOAT = DMM_MSG + 3,
        DMM_DOCKALL = DMM_MSG + 4,
        DMM_FLOATALL = DMM_MSG + 5,
        DMM_MOVE = DMM_MSG + 6,
        DMM_UPDATEDISPINFO = DMM_MSG + 7,
        DMM_GETIMAGELIST = DMM_MSG + 8,
        DMM_GETICONPOS = DMM_MSG + 9,
        DMM_DROPDATA = DMM_MSG + 10,
        DMM_MOVE_SPLITTER = DMM_MSG + 11,
        DMM_CANCEL_MOVE = DMM_MSG + 12,
        DMM_LBUTTONUP = DMM_MSG + 13,

        DMN_FIRST = 1050,
        DMN_CLOSE = DMN_FIRST + 1,
        //nmhdr.Code = DWORD(DMN_CLOSE, 0));
        //nmhdr.hwndFrom = hwndNpp;
        //nmhdr.IdFrom = ctrlIdNpp;

        DMN_DOCK = DMN_FIRST + 2,
        DMN_FLOAT = DMN_FIRST + 3
        //nmhdr.Code = DWORD(DMN_XXX, int newContainer);
        //nmhdr.hwndFrom = hwndNpp;
        //nmhdr.IdFrom = ctrlIdNpp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct toolbarIcons
    {
        public IntPtr hToolbarBmp;
        public IntPtr hToolbarIcon;
    }

    // All 3 handles below should be set so the icon will be displayed correctly if toolbar icon sets are changed by users, also in dark mode
    [StructLayout(LayoutKind.Sequential)]
    public struct toolbarIconsWithDarkMode
    {
        public IntPtr hToolbarBmp;
        public IntPtr hToolbarIcon;
        public IntPtr hToolbarIconDarkMode;
    }
}
