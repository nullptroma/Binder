using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LowLevelControls.Natives;
using LowLevelControls;
using System.Reflection;

namespace Binder
{
    /// <summary>
    /// Логика взаимодействия для RecordWindow.xaml
    /// </summary>
    public partial class RecordWindow : System.Windows.Window
    {
        KeyboardHook kbdHook = new KeyboardHook();
        LowLevelControls.MouseHook msHook = new LowLevelControls.MouseHook();
        GlobalHook gh = new GlobalHook();
        Queue<string> cmds = new Queue<string>();
        double perc = 0;
        double d;
        string cursorMeth = "";
        string keyDown = "";
        string keyUp= "";

        public RecordWindow()
        {
            InitializeComponent();
            kbdHook.NativeHookProcEvent += KbdHook_NativeHookProcEvent;
            msHook.NativeHookProcEvent += MsHook_NativeHookProcEvent;
            this.Closing += Closing_Window;
            gh.KeyDown += KeyDown;
            gh.KeyUp += KeyUp;

        }
        bool alt=false;
        bool hooked = false;
        new void KeyDown(object sender, GlobalHook.MyEventArgs e)
        {
            if (e.e.Key == Key.LeftAlt)
                alt = true;
            if (e.e.Key == Key.S && alt)
            {
                if (hooked)
                {
                    UNHOOK();
                    RecordText.Text += string.Join("", cmds);
                    RecordingLab.Content = "";
                }
                else
                {
                    cmds.Clear();
                    HOOK();
                    RecordingLab.Content = "Идёт запись...";
                }
                hooked = !hooked;
            }    
        }

        new void KeyUp(object sender, GlobalHook.MyEventArgs e)
        {
            if (e.e.Key == Key.LeftAlt)
                alt = false;
        }


        void HOOK()
        {
            if(!kbdHook.hookInstalled)
            {
                if (MouseImitationMeth.SelectedIndex == 0)
                    cursorMeth = @"SetCursorPos({0}, {1});";
                else if (MouseImitationMeth.SelectedIndex == 1)
                    cursorMeth = @"MoveCursor({0}, {1}, 0);";

                if (KeyboardImitationMeth.SelectedIndex == 0)
                {
                    keyDown = @"AHKExecRaw(""Send {{{0} down}}"");";
                    keyUp = @"AHKExecRaw(""Send {{{0} up}}"");";
                }
                else if (KeyboardImitationMeth.SelectedIndex == 1)
                {
                    keyDown = @"KeyDown(""{0}"");";
                    keyUp = @"KeyUp(""{0}"");";
                }
                
                if (!CheckPerc())
                {
                    System.Windows.MessageBox.Show("Запишите проценты нормально");
                    return;
                }
                kbdHook.InstallGlobalHook();
                msHook.InstallGlobalHook();
                perc = Double.Parse(Perc.Text);
                d = 100.0 / perc;
                count = 0;
                lastCMD = DateTime.Now;
            }
        }
        void UNHOOK()
        {
            if (kbdHook.hookInstalled)
            {
                kbdHook.UninstallGlobalHook();
                msHook.UninstallGlobalHook();
            }
            
        }


        DateTime lastCMD;
        void AddCommand(string cmd)
        {
            var del = (int)((DateTime.Now - lastCMD).TotalMilliseconds);
            if (del > 0)
                cmds.Enqueue($"Delay({del});" + Environment.NewLine);
            cmds.Enqueue(cmd + Environment.NewLine); 
            lastCMD = DateTime.Now;
            for (var i = this.Scroll.ContentVerticalOffset; i < this.Scroll.ScrollableHeight; i++)
            {
                Scroll.ScrollToVerticalOffset(i);
            }
        }
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowThreadProcessId(
            [In] IntPtr hWnd,
            [Out, Optional] IntPtr lpdwProcessId
            );

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(int idThread);
        /// <summary>
        /// Вернёт Id раскладки.
        /// </summary>
        static ushort GetKeyboardLayout()
        {
            return (ushort)GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(),
                                                                      IntPtr.Zero));
        }


        [DllImport("user32.dll")]
        public static extern int ToUnicode(uint virtualKeyCode, uint scanCode,
    byte[] keyboardState,
    [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)]
    StringBuilder receivingBuffer,
    int bufferSize, uint flags, int lang);

        static string GetCharsFromKeys(Keys keys, bool shift, bool altGr)
        {
            
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
                keyboardState[(int)Keys.ShiftKey] = 0xff;
            if (altGr)
            {
                keyboardState[(int)Keys.ControlKey] = 0xff;
                keyboardState[(int)Keys.Menu] = 0xff;
            }
            ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0, GetKeyboardLayout());

            return buf.ToString().Trim()!=""? buf.ToString() : keys.ToString();
        }

        bool shiftKey = false;
        bool altKey = false;

        IntPtr KbdHook_NativeHookProcEvent(int nCode, IntPtr wParam, IntPtr lParam)
        {
            KBDLLHOOKSTRUCT kbd =
                (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
            
            switch ((WM)wParam)
            {
                case WM.KEYDOWN:
                case WM.SYSKEYDOWN:
                    if ((Keys)kbd.vkCode == Keys.Alt)
                        altKey = true;
                    if ((Keys)kbd.vkCode == Keys.LShiftKey)
                        shiftKey = true;
                    AddCommand(string.Format(keyDown, GetCharsFromKeys((Keys)kbd.vkCode, shiftKey, altKey)));
                    break;
                case WM.KEYUP:
                case WM.SYSKEYUP:
                    if ((Keys)kbd.vkCode == Keys.Alt)
                        altKey = false;
                    if ((Keys)kbd.vkCode == Keys.LShiftKey)
                        shiftKey = false;
                    AddCommand(string.Format(keyUp, GetCharsFromKeys((Keys)kbd.vkCode, shiftKey, altKey)));
                    break;
            }
            return IntPtr.Zero;
        }
        double count = 0;
        IntPtr MsHook_NativeHookProcEvent(int nCode, IntPtr wParam, IntPtr lParam)
        {
            MSLLHOOKSTRUCT ms =
                (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
            
            switch ((WM)wParam)
            {
                case WM.LBUTTONDOWN:
                    AddCommand(string.Format(cursorMeth, ms.pt.x, ms.pt.y));
                    AddCommand($"MouseEvent(\"LEFTDOWN\");");
                    break;
                case WM.LBUTTONUP:
                    AddCommand(string.Format(cursorMeth, ms.pt.x, ms.pt.y));
                    AddCommand($"MouseEvent(\"LEFTUP\");");
                    break;
                case WM.RBUTTONDOWN:
                    AddCommand(string.Format(cursorMeth, ms.pt.x, ms.pt.y));
                    AddCommand($"MouseEvent(\"RIGHTDOWN\");");
                    break;
                case WM.RBUTTONUP:
                    AddCommand(string.Format(cursorMeth, ms.pt.x, ms.pt.y));
                    AddCommand($"MouseEvent(\"RIGHTUP\");");
                    break;
                case WM.MBUTTONDOWN:
                    AddCommand(string.Format(cursorMeth, ms.pt.x, ms.pt.y));
                    AddCommand($"MouseEvent(\"MIDDLEDOWN\");");
                    break;
                case WM.MBUTTONUP:
                    AddCommand(string.Format(cursorMeth, ms.pt.x, ms.pt.y));
                    AddCommand($"MouseEvent(\"MIDDLEUP\");");
                    break;
                case WM.MOUSEMOVE:
                    if (RecordMove.IsChecked.Value)
                        if (count>d)
                        {
                            count = 0;
                            AddCommand(string.Format(cursorMeth, ms.pt.x, ms.pt.y));
                        }
                    count++;
                    break;
            }
            return IntPtr.Zero;
        }

        void Closing_Window(object sender, EventArgs e)
        {
            if(kbdHook.hookInstalled)
                kbdHook.UninstallGlobalHook();
            if(msHook.hookInstalled)
                msHook.UninstallGlobalHook();
            gh.KeyDown -= KeyDown;
            gh.KeyUp -= KeyUp;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
           
            RecordText.Text = "";
        }

        private void Perc_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Clear.Focus();

        }

        bool CheckPerc()
        {
            double res = 0;
            return Double.TryParse(Perc.Text, out res);
        }

        private void Perc_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var data = new System.Windows.Forms.DataObject();
            Thread thread;
            data.SetData(System.Windows.Forms.DataFormats.UnicodeText, true, RecordText.Text);
            thread = new Thread(() => System.Windows.Forms.Clipboard.SetDataObject(data, true));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            System.Windows.Forms.MessageBox.Show("Скопировано");
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            MouseImitationMeth.SelectedIndex = 0;
            KeyboardImitationMeth.SelectedIndex = 0;
        }
    }
}
