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
using Microsoft.SqlServer.Server;

namespace ScriptsLibrary
{
    static public class StringScripts
    {
        [Description(Desc = "ToString(object o1, object o2...) - возвращает строковое представление объекта(ов)(можно использовать для объединения строк).")]
        public static string ToString(params object[] ps)
        {
            if(ps.Length!=0)
                return string.Join("", ps);
            return "";
        }


        [Description(Desc = "GetDesktopPath() - возвращает путь до рабочего стола.")]
        public static string GetDesktopPath(params object[] ps)
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        [Description(Desc = "ReplaceString(string str, string subStr, string replace) - возвращает строку с заменёнными subStr на replace.")]
        public static string ReplaceString(params object[] ps)
        {
            try
            {
                return ps[0].ToString().Replace(ps[1].ToString(), ps[2].ToString());
            }
            catch { MessageBox.Show("В ReplaceString недостаточно аргументов"); return ""; }
        }


        [Description(Desc = "GetUserName() - возвращает имя текущего пользователя.")]
        public static string GetUserName(params object[] ps)
        {
            return Environment.UserName;
        }


        [Description(Desc = "GetKeysHelp() - возвращает помощь по кнопкам.")]
        public static string GetKeysHelp(params object[] ps)
        {
            string ans = "";
            foreach (var k in Enum.GetValues(typeof(Keys)))
            {
                ans += k.ToString() + ";     ";
            }
            return ans;
        }

        [Description(Desc = "GetSendKeysHelp() - возвращает помощь для скрипта SendKeysWait.")]
        public static string GetSendKeysHelp(params object[] ps)
        {
            return "SendKeysWait() посылает активному окну последовательность клавиш. Чтобы отправить обычный текст просто запишите его так: SendKeysWait(\"Hello\"). Чтобы имитировать нажатие например CTRL+C: SendKeysWait(^{C}).";
        }

        
        [Description(Desc = "GetMouseEventsHelp() - возвращает помощь для скрипта MouseEvent.")]
        public static string GetMouseEventsHelp(params object[] ps)
        {
            string ans = "";
            foreach (var k in Enum.GetValues(typeof(Meths.MouseEventFlags)))
            {
                ans += k.ToString() + ";     ";
            }
            return ans;
        }

        [Description(Desc = "GetClipboardText() - возвращает текст из буфера обмена Windows.")]
        public static string GetClipboardText(params object[] ps)
        {
            return Clipboard.GetText();
        }



        [Description(Desc = "LayoutSimbols(string text) - возвращает текст с изменённой раскладкой русский-английский по QWERTY.")]
        public static string LayoutSimbols(params object[] ps)
        {
            if (ps.Length == 0)
            {
                return "";
            }
            string str = ps[0].ToString();
            for (int i = 0; i < str.Length; i++)
            {
                foreach ((char, char) chrs in Vars.LayoutSimbols)
                {

                    if (str[i] == chrs.Item1)
                    {
                        str = str.Remove(i, 1).Insert(i, chrs.Item2.ToString());
                        break;
                    }
                    else if (str[i] == chrs.Item2)
                    {
                        str = str.Remove(i, 1).Insert(i, chrs.Item1.ToString());
                        break;
                    }

                }
            }

            return str;
        }
    }
}
