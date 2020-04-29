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

namespace ScriptsLibrary
{
    static public class DoubleScripts
    {
        [Description(Desc = "SumDouble(double n1, double n2...) - возвращает результат сложения всех переданных аргументов.")]
        public static double SumDouble(params object[] ps)
        {
            try
            {
                double answer = double.Parse(ps[0].ToString());
                for (int i = 1; i < ps.Length; i++)
                    answer += double.Parse(ps[i].ToString());
                return answer;
            }
            catch (InvalidCastException) { MessageBox.Show("В SumDouble переданы значения типа, несоответвующего double"); return 0; }
            catch (IndexOutOfRangeException) { MessageBox.Show("В SumDouble не переданы агрументы"); return 0; }
        }

        [Description(Desc = "MinusDouble(double num, double n1...) - возвращает результат вычитания всех чисел начиная с n1 из num.")]
        public static double MinusDouble(params object[] ps)
        {
            try
            {
                double answer = double.Parse(ps[0].ToString());
                for (int i = 1; i < ps.Length; i++)
                    answer -= double.Parse(ps[i].ToString());
                return answer;
            }
            catch (InvalidCastException){ MessageBox.Show("В MinusDouble переданы значения типа, несоответвующего double"); return 0; }
            catch (IndexOutOfRangeException){ MessageBox.Show("В MinusDouble не переданы агрументы"); return 0; }
        }

        [Description(Desc = "MultyplyDouble(double num1, double num2) - возвращает результат умножения чисел num1 и num2.")]
        public static double MultyplyDouble(params object[] ps)
        {
            try
            {
                return double.Parse(ps[0].ToString()) * double.Parse(ps[1].ToString());
            }
            catch (InvalidCastException) { MessageBox.Show("В MultyplyDouble переданы значения типа, несоответвующего double"); return 0; }
            catch (IndexOutOfRangeException) { MessageBox.Show("В MultyplyDouble не переданы агрументы"); return 0; }
        }

        [Description(Desc = "DivDouble(double num1, double num2) - возвращает результат деления чисела num1 на num2.")]
        public static double DivDouble(params object[] ps)
        {
            try
            {
                return double.Parse(ps[0].ToString()) / double.Parse(ps[1].ToString());
            }
            catch (InvalidCastException) { MessageBox.Show("В DivDouble переданы значения типа, несоответвующего double"); return 0; }
            catch (IndexOutOfRangeException) { MessageBox.Show("В DivDouble не переданы агрументы"); return 0; }
        }

        [Description(Desc = "ConvertStringToDouble(string str) - пытается конвернитровать string в double.")]
        public static double ConvertStringToDouble(params object[] ps)
        {
            try
            {
                return double.Parse(ps[0].ToString().Replace(".", ","));
            }
            catch (IndexOutOfRangeException) { MessageBox.Show("В ConvertStringToDouble не переданы агрументы"); return 0; }
            catch (FormatException) { MessageBox.Show("В ConvertStringToDouble передан неподходящий аргумент"); return 0; }
        }
    }
}
