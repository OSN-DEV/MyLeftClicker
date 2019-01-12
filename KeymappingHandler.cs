using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyLeftClicker.AppSettingData;
using System.Windows.Forms;
using System.Diagnostics;

namespace MyLeftClicker {
    class KeymappingHandler {

        #region Declaration - Dll
        private static class NativeMethods {
            // callback 
            public delegate IntPtr KeyboardGlobalHookCallback(int code, uint msg, ref KBDLLHOOKSTRUCT hookData);

            // https://msdn.microsoft.com/ja-jp/library/cc430103.aspx
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr SetWindowsHookEx(int idHook, KeyboardGlobalHookCallback lpfn, IntPtr hMod, uint dwThreadId);

            // https://msdn.microsoft.com/ja-jp/library/cc429591.aspx
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, uint msg, ref KBDLLHOOKSTRUCT hookData);

            // https://msdn.microsoft.com/ja-jp/library/cc430120.aspx
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            // https://msdn.microsoft.com/ja-jp/library/cc364822.aspx
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        private static IntPtr _keyEventHandle;
        private static event NativeMethods.KeyboardGlobalHookCallback _hookCallback;
        #endregion

        #region Declaration
        private static class Const {
            public const int Action = 0;            // 0のみフックするのがお約束らしい
            public const int HookTypeLL = 13;       // WH_KEYBOARD_LL
        }

        private static class KeyStroke {
            public const int KeyDown = 0x100;
            public const int KeyUp = 0x101;
            public const int SysKeyDown = 0x104;
            public const int SysKeyup = 0x105;
        }
        private static class Flags {
            public const uint None = 0x16;
            public const uint KeyDown = 0x00;
            public const uint KeyUp = 0x02;
            public const uint ExtendeKey = 0x01;     //  拡張コード(これを設定することで修飾キーも有効になる)
            public const uint Unicode = 0x04;
            public const uint ScanCode = 0x08;
        }
        private static class ExtraInfo {
            public const int SendKey = 1;
            public const int LLKHF_EXTENDED = 0x00000001;
        }

        private class KeyData {
            public byte[] KeySet;
            public uint Flag;
            public KeyData(byte[] keySet, uint flag) {
                this.KeySet = keySet;
                this.Flag = flag;
            }
        }
        private static class MouseEvent {
            public const int LeftDown = 0x02;
            public const int LeftUp = 0x04;
            public const int RightDown = 0x08;
            public const int RightUp = 0x10;
        }


        /// <summary>
        ///  key set of index
        /// </summary>
        private static class KeySetIndex {
            public const int VirtualKey = 0;
            public const int ScanCode = 1;
        }

        public static int ClickCount { set {
                _timer.Interval = 1000 / value;
            } }

        private static Timer _timer = new Timer();
        #endregion

        #region Constructor

        #endregion

        #region Public Property
        public static bool IsHooking {
            get;
            private set;
        }
        #endregion

        #region Public Method
        /// <summary>
        /// set up timer
        /// </summary>
        public static void SetUp() {
            _timer.Tick += Timer_Tick;
            _timer.Enabled = false;
        }

        /// <summary>
        ///  start global hook.
        /// </summary>
        public static void Start() {
            if (IsHooking) {
                return;
            }

            _timer.Enabled = false;
            IsHooking = true;

            _hookCallback = HookProcedure;
            IntPtr hinst = System.Runtime.InteropServices.Marshal.GetHINSTANCE(typeof(KeymappingHandler).Assembly.GetModules()[0]);

            _keyEventHandle = NativeMethods.SetWindowsHookEx(Const.HookTypeLL, _hookCallback, hinst, 0);
            if (_keyEventHandle == IntPtr.Zero) {
                IsHooking = false;
                throw new System.ComponentModel.Win32Exception();
            }
        }

        /// <summary>
        /// stop global hook.
        /// </summary>
        public static void Stop() {
            if (!IsHooking) {
                return;
            }

            _timer.Enabled = false;
            if (_keyEventHandle != IntPtr.Zero) {
                IsHooking = false;
                NativeMethods.UnhookWindowsHookEx(_keyEventHandle);
                _keyEventHandle = IntPtr.Zero;
                _hookCallback -= HookProcedure;
            }
        }
        #endregion

        #region Privater Method
        /// <summary>
        /// hook
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="hookData"></param>
        /// <returns></returns>
        private static IntPtr HookProcedure(int code, uint msg, ref KBDLLHOOKSTRUCT hookData) {
            ushort scanCode = (ushort)hookData.scanCode;

            if (Const.Action != code || (IntPtr)ExtraInfo.SendKey == hookData.dwExtraInfo) {
                goto ExitProc;
            }
            // 例えば←とテンキーの4はスキャンコードが同一。テンキーは拡張フラグがオフなので、
            // それ前提の判断とする。
            if ((hookData.flags & ExtraInfo.LLKHF_EXTENDED) == ExtraInfo.LLKHF_EXTENDED) {
                goto ExitProc;
            }
            if (KeyStroke.KeyDown == msg && scanCode == KeySet.ControlL[KeySetIndex.ScanCode]) {
                _timer.Enabled = !_timer.Enabled;
            }

            ExitProc:
            return NativeMethods.CallNextHookEx(_keyEventHandle, code, msg, ref hookData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyList"></param>
        private static void SendKey(List<KeyData> keyList) {
            foreach (var key in keyList) {
                NativeMethods.keybd_event(
                    key.KeySet[KeySetIndex.VirtualKey], key.KeySet[KeySetIndex.ScanCode], key.Flag, (UIntPtr)ExtraInfo.SendKey);
            }
        }

        private static void Timer_Tick(Object sender, EventArgs e) {
            Debug.WriteLine("tick!!!!!!");
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            NativeMethods.mouse_event(MouseEvent.LeftDown | MouseEvent.LeftUp, X, Y, 0, 0);
        }
        #endregion
    }
}
