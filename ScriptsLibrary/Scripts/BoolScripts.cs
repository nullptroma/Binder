using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Input;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.CodeDom;

namespace ScriptsLibrary
{
    static public class BoolScripts
    {
        [Description(Desc = "HideDesktopBackground() - скрывает фон рабочего стола. Возвращает результат операции.")]
        public static bool HideDesktopBackground(params object[] ps)
        {
            bool show = true;
            IntPtr hWin = Meths.FindWindow("Progman", null);
            if (hWin != IntPtr.Zero)
                return Meths.ShowWindow(hWin, show ? 0 : 5);

            return false;
        }

        [Description(Desc = "ShowDesktopBackground() - показывает фон рабочего стола. Возвращает результат операции.")]
        public static bool ShowDesktopBackground(params object[] ps)
        {
            bool show = false;
            IntPtr hWin = Meths.FindWindow("Progman", null);
            if (hWin != IntPtr.Zero)
                return Meths.ShowWindow(hWin, show ? 0 : 5);

            return false;
        }

        

        [Description(Desc = "MoreThan(object left, object right) - сравнивает 2 значения и возвращает true если left>right.")]
        public static bool MoreThan(params object[] ps)
        {
            if (ps.Length < 2)
            {
                MessageBox.Show("В MoreThan передано недостаточно аргументов");
                throw new Exception("В MoreThan передано недостаточно аргументов");
            }

            return Meths.CompareUniversal(ps[0], ps[1]) == 1;
        }

        [Description(Desc = "LessThan(object left, object right) - сравнивает 2 значения и возвращает true если left<right.")]
        public static bool LessThan(params object[] ps)
        {
            if (ps.Length < 2)
            {
                MessageBox.Show("В LessThan передано недостаточно аргументов");
                throw new Exception("В LessThan передано недостаточно аргументов");
            }

            return Meths.CompareUniversal(ps[0], ps[1])==-1;
        }

        [Description(Desc = "Equal(object left, object right) - сравнивает 2 значения и возвращает true если left==right.")]
        public static bool Equal(params object[] ps)
        {
            if (ps.Length < 2)
            {
                MessageBox.Show("В Equal передано недостаточно аргументов");
                throw new Exception("В Equal передано недостаточно аргументов");
            }
            try
            {
                return object.Equals(ps[0], ps[1]);
            }
            catch { MessageBox.Show("В Equal нельзя сравнить типы " + ps[0].GetType().Name + " " + ps[1].GetType().Name); return false; }
        }

        [Description(Desc = "And(bool b1, bool b2....) - применяет операцию AND ко все параметрам и возвращает результат.")]
        public static bool And(params object[] ps)
        {
            if (ps.Length == 0)
            {
                MessageBox.Show("В And передано недостаточно аргументов");
                throw new Exception("В And передано недостаточно аргументов");
            }
            try
            {
                foreach (var bs in ps)
                {
                    if ((bool)bs == false)
                        return false;
                }
                return true;
            }
            catch { MessageBox.Show("В And передано значение типа, несоответсвующего Boolean "); return false; }
        }

        [Description(Desc = "Or(bool b1, bool b2....) - применяет операцию OR ко все параметрам и возвращает результат.")]
        public static bool Or(params object[] ps)
        {
            if (ps.Length == 0)
            {
                MessageBox.Show("В Or передано недостаточно аргументов");
                throw new Exception("В Or передано недостаточно аргументов");
            }
            try
            {
                foreach (var bs in ps)
                {
                    if ((bool)ps[0])
                        return true;
                }
                return false;
            }
            catch { MessageBox.Show("В Or передано значение типа, несоответсвующего Boolean "); return false; }
        }

        [Description(Desc = "Not(bool b) - возвращает инвертированное значение b.")]
        public static bool Not(params object[] ps)
        {
            try
            {
                return !(bool)ps[0];
            }
            catch (InvalidCastException) { MessageBox.Show("В NoBoolean передан тип, несоотвутствующий Boolean"); return false; }
            catch (IndexOutOfRangeException) { MessageBox.Show("В NoBoolean не передан аргумент"); return false; }
        }

        
    }
}
