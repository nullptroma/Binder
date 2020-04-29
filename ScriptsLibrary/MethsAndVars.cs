using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Runtime.InteropServices;
using System.CodeDom;
using System.Windows.Forms;

namespace ScriptsLibrary
{
    public class Plugin
    {
        public IEnumerable<Assembly> GetNames()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class Description : Attribute
    {
        public string Desc { get; set; }

        public Description()
        {
        }
    }


    static public class Vars
    {
        static public List<(char, char)> LayoutSimbols = new List<(char, char)>(){
        ('^',':'),('&','?'),

        ('q','й'),('w','ц'),('e','у'),('r','к'),('t','е'),('y','н'),('u','г'),('i','ш'),('o','щ'),('p','з'),
        ('[','х'),(']','ъ'),('a','ф'),('s','ы'),('d','в'),('f','а'),('g','п'),('h','р'),('j','о'),('k','л'),
        ('l','д'),(';','ж'),('\'','э'),('z','я'),('x','ч'),('c','с'),('v','м'),('b','и'),('n','т'),('m','ь'),
        (',','б'),('.','ю'),

        ('Q','Й'),('W','Ц'),('E','У'),('R','К'),('T','Е'),('Y','Н'),('U','Г'),('I','Ш'),('O','Щ'),('P','З'),
        ('[','Х'),(']','Ъ'),('A','Ф'),('S','Ы'),('D','В'),('F','А'),('G','П'),('H','Р'),('J','О'),('K','Л'),
        ('L','Д'),(':','Ж'),('\'','Э'),('Z','Я'),('X','Ч'),('C','С'),('V','М'),('B','И'),('N','Т'),('M','Ь'),
        ('<','Б'),('>','Ю'),

        ('@','"'),('#','№'),('?',','),('$',';')

        };
    }

    static public class Meths
    {

        public static int CompareUniversal(object l, object r)
        {
            string lname = l.GetType().Name;
            string rname = r.GetType().Name;
            if (lname == rname)
            {
                if (lname == "Double")
                {
                    return ((double)l).CompareTo((double)r);
                }
                if (lname == "Int32")
                {
                    return ((int)l).CompareTo((int)r);
                }
                if (lname == "String")
                {
                    return ((string)l).CompareTo((string)r);
                }
                if (lname == "Boolean")
                {
                    return ((bool)l).CompareTo((bool)r);
                }
            }
            if (lname == "Double")
            {
                if (rname == "Int32")
                {
                    return ((double)l).CompareTo(Convert.ToDouble(r));
                }
            }
            if (lname == "Int32")
            {
                if (rname == "Double")
                {
                    return (Convert.ToDouble(l)).CompareTo((double)r);
                }
            }

            MessageBox.Show("Нельзя сравнить значения типов " + lname + " и " + rname);
            throw new Exception("Нельзя сравнить значения типов " + lname + " и " + rname);
        }


        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 pid);


        [DllImport("user32.dll", SetLastError = true)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);


        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point pt);


        [DllImport("user32.dll")]
        public static extern void SetCursorPos(int x, int y);


        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);


        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public delegate bool EnumCallback(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, [Out] StringBuilder buf, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindWindow(string lpsz1, string lpsz2);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumCallback lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GWConstants iCmd);


        public static string GetClassNameFromHWND(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            int len = GetClassName(hWnd, sb, sb.Capacity);
            if (len > 0)
                return sb.ToString(0, len);

            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        public static bool EnumWins(IntPtr hWnd, IntPtr lParam)
        {
            if (hWnd != IntPtr.Zero)
            {
                IntPtr hDesk = GetWindow(hWnd, GWConstants.GW_CHILD);
                if (hDesk != IntPtr.Zero && GetClassNameFromHWND(hDesk) == "SHELLDLL_DefView")
                {
                    hDesk = GetWindow(hDesk, GWConstants.GW_CHILD);
                    if (hDesk != IntPtr.Zero && GetClassNameFromHWND(hDesk) == "SysListView32")
                    {
                        ShowWindow(hDesk, lParam.ToInt32());
                        return false;
                    }
                }
            }
            return true;
        }

        public enum GWConstants : int
        {
            GW_HWNDFIRST,
            GW_HWNDLAST,
            GW_HWNDNEXT,
            GW_HWNDPREV,
            GW_OWNER,
            GW_CHILD,
            GW_MAX
        }

        
    }



}

