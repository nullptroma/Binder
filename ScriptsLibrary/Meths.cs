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

    static public class Meths
    {

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

