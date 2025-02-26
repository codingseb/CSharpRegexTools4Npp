using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace RegexDialog
{
    public class KeyboardHookManager
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private static IntPtr _hookID = IntPtr.Zero;
        private static Window _targetWindow;
        private static LowLevelKeyboardProc _proc;
        private static bool _isHookInstalled = false;

        public static event EventHandler<KeyPressedEventArgs> KeyPressed;

        public static void Initialize(Window window)
        {
            _targetWindow = window;
            _proc = HookCallback;

            // S'abonner aux événements de focus de la fenêtre
            window.Activated += Window_Activated;
            window.Deactivated += Window_Deactivated;
        }

        private static void Window_Activated(object sender, EventArgs e)
        {
            InstallHook();
        }

        private static void Window_Deactivated(object sender, EventArgs e)
        {
            UninstallHook();
        }

        private static void InstallHook()
        {
            if (!_isHookInstalled)
            {
                _hookID = SetHook(_proc);
                _isHookInstalled = true;
            }
        }

        private static void UninstallHook()
        {
            if (_isHookInstalled)
            {
                UnhookWindowsHookEx(_hookID);
                _isHookInstalled = false;
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var windowHandle = GetForegroundWindow();
                var activeWindow = HwndSource.FromHwnd(windowHandle)?.RootVisual as Window;

                // Vérifier si la fenêtre active est notre fenêtre cible
                if (activeWindow == _targetWindow)
                {
                    if ((wParam == (IntPtr)WM_KEYDOWN) || (wParam == (IntPtr)WM_SYSKEYDOWN))
                    {
                        int vkCode = Marshal.ReadInt32(lParam);
                        bool isCtrlPressed = (GetKeyState(VK_CONTROL) & 0x8000) != 0;
                        bool isAltPressed = (GetKeyState(VK_MENU) & 0x8000) != 0;
                        bool isShiftPressed = (GetKeyState(VK_SHIFT) & 0x8000) != 0;

                        Key wpfKey = KeyInterop.KeyFromVirtualKey(vkCode);

                        var args = new KeyPressedEventArgs(
                            wpfKey,
                            isCtrlPressed,
                            isAltPressed,
                            isShiftPressed
                        );

                        KeyPressed?.Invoke(null, args);

                        // Si l'événement a été géré, on empêche sa propagation
                        if (args.Handled)
                        {
                            return (IntPtr)1;
                        }
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12;    // ALT
        private const int VK_SHIFT = 0x10;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    public class KeyPressedEventArgs : EventArgs
    {
        public Key Key { get; private set; }
        public bool Control { get; private set; }
        public bool Alt { get; private set; }
        public bool Shift { get; private set; }
        public bool Handled { get; set; }

        public KeyPressedEventArgs(Key key, bool control, bool alt, bool shift)
        {
            Key = key;
            Control = control;
            Alt = alt;
            Shift = shift;
            Handled = false;
        }
    }
}