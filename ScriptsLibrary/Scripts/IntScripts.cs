using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace ScriptsLibrary
{
    static public class IntScripts
    {
        [Description(Desc = "SumInt(int n1, int n2...) - возвращает результат сложения всех переданных аргументов.")]
        public static int SumInt(params object[] ps)
        {
            try
            {
                return ps.Cast<int>().Sum();
            }
            catch (InvalidCastException) { MessageBox.Show("В SumInt переданы значения типа, несоответвующего int"); return 0; }
            catch (IndexOutOfRangeException) { MessageBox.Show("В SumInt не переданы агрументы"); return 0; }
        }

        [Description(Desc = "MinusInt(int num, int n1...) - возвращает результат вычитания всех чисел начиная с n1 из num.")]
        public static int MinusInt(params object[] ps)
        {
            try
            {
                int answer = (int)ps[0];
                for (int i = 1; i < ps.Length; i++)
                    answer -= (int)ps[i];
                return answer;
            }
            catch (InvalidCastException) { MessageBox.Show("В MinusInt переданы значения типа, несоответвующего int"); return 0; }
            catch (IndexOutOfRangeException) { MessageBox.Show("В MinusInt не переданы агрументы"); return 0; }
        }

        [Description(Desc = "MultyplyInt(int num1, int num2) - возвращает результат умножения чисел num1 и num2.")]
        public static int MultyplyInt(params object[] ps)
        {
            try
            {
                return (int)ps[0] * (int)ps[1];
            }
            catch (InvalidCastException) { MessageBox.Show("В MultyplyInt переданы значения типа, несоответвующего int"); return 0; }
            catch (IndexOutOfRangeException) { MessageBox.Show("В MultyplyInt не переданы агрументы"); return 0; }
        }

        [Description(Desc = "DivInt(int num1, int num2) - возвращает результат деления чисела num1 на num2.")]
        public static int DivInt(params object[] ps)
        {
            try
            {
                return (int)ps[0] / (int)ps[1];
            }
            catch (InvalidCastException) { MessageBox.Show("В DivInt переданы значения типа, несоответвующего int"); return 0; }
            catch (IndexOutOfRangeException) { MessageBox.Show("В DivInt не переданы агрументы"); return 0; }
        }

        [Description(Desc = "ConvertFromStringToInt(string str) - пытается конвернитровать string в int.")]
        public static int ConvertFromStringToInt(params object[] ps)
        {
            try
            {
                return int.Parse(ps[0].ToString());
            }
            catch (IndexOutOfRangeException) { MessageBox.Show("В ConvertFromStringToInt не передан подходящий агрумент"); return 0; }
            catch (FormatException) { MessageBox.Show("В ConvertFromStringToInt передан неподходящий аргумент"); return 0; }
        }

        [Description(Desc = "Compare(object l, object r) - производит компарирование со значениями l и r.")]
        public static int Compare(params object[] ps)
        {
            if (ps.Length < 2)
            {
                MessageBox.Show("В Compare передано недостаточно аргументов");
                throw new Exception("В Compare передано недостаточно аргументов");
            }
            return Meths.CompareUniversal(ps[0], ps[1]);
        }

        [Description(Desc = "GetProcessID(string name) - возвращает ID процесса с именем name. Если процесс не найден вернёт -1.")]
        public static int GetProcessID(params object[] ps)
        {
            if (ps.Length == 0)
            {
                MessageBox.Show("В GetProcessID передано недостаточно аргументов");
                throw new Exception("В GetProcessID передано недостаточно аргументов");
            }
            return Process.GetProcessesByName(ps[0].ToString()).Length==0 ? -1 : Process.GetProcessesByName(ps[0].ToString()).First().Id;
        }

        [Description(Desc = "GetProcessIDForgedWindow() - возвращает ID процесса активного окна.")]
        public static int GetProcessIDForgedWindow(params object[] ps)
        {
            int id = -1;
            Meths.GetWindowThreadProcessId(Meths.GetForegroundWindow(), ref id);
            return id;
        }

        [Description(Desc = "GetCursorPosX() - возвращает позицию курсора по координате X.")]
        public static int GetCursorPosX(params object[] ps)
        {
            return Cursor.Position.X;
        }

        [Description(Desc = "GetCursorPosY() - возвращает позицию курсора по координате Y.")]
        public static int GetCursorPosY(params object[] ps)
        {
            return Cursor.Position.Y;
        }
    }
}
