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
    static public class VoidScripts
    {
        [Description(Desc = "MsgBox(string title, object text1...) - выводит сообщение, объединяя все переданные аргументы.")]
        public static object MsgBox(params object[] ps)
        {
            if (ps.Length > 1)
            {
                string str = "";
                for (int i = 1; i < ps.Length; i++)
                    str += ps[i].ToString();
                MessageBox.Show(str, ps[0].ToString());
            }
            else
            {
                string str = "";
                foreach (var r in ps)
                    str += r.ToString();
                MessageBox.Show(str);
            }
            return ps;
        }


        [Description(Desc = "ShowHideDesktopIcons(bool show) - показывает/скрывает значки рабочего стола.")]
        public static object ShowHideDesktopIcons(params object[] ps)
        {
            try
            {
                if (ps.Length == 0)
                    return ps;
                Meths.EnumWindows(new Meths.EnumCallback(Meths.EnumWins), (bool)ps[0] ? (IntPtr)5 : IntPtr.Zero);
                return ps;
            }
            catch { MessageBox.Show("В MsgBox первый аргумент типа, несоотвествующего Boolean"); return ps; }
        }

        [Description(Desc = "Delay(int ms) - приостанавливает выполнение скрипта на ms миллисекунд.")]
        public static object Delay(params object[] ps)
        {
            try
            {
                if (ps.Length == 0)
                    return ps;
                Thread.Sleep((int)ps[0]);
                return ps;
            }
            catch { MessageBox.Show("В Delay первый аргумент типа, несоотвествующего Int"); return ps; }
        }

        [Description(Desc = "KillProcessesByName(string name1, string name2...) - находит процессы с именами name... и останавливает их.")]
        public static object KillProcessesByName(params object[] ps)
        {
            if (ps.Length == 0)
                return ps;
            foreach (var n in ps)
            {
                var r = Process.GetProcessesByName(n.ToString());
                r.AsParallel().ForAll(p => p.Kill());
            }
            return ps;
        }

        [Description(Desc = "KillProcessesByID(int id1, int id2...) - находит процессы с ID id1... и останавливает их.")]
        public static object KillProcessesByID(params object[] ps)
        {
            if (ps.Length == 0)
                return ps;
            foreach (var n in ps)
            {
                var r = Process.GetProcessById((int)n);
                r.Kill();
            }
            return ps;
        }

        [Description(Desc = "ProcessRun(string path) - запускает процесс по адрессу path(ссылки, приложения, файлы, пути в проводнике).")]
        public static object ProcessRun(params object[] ps)
        {
            if (ps.Length == 0)
                return ps;
            try
            {
                Process.Start(ps[0].ToString());
            }
            catch { MessageBox.Show("ProcessRun не удалось запустить процесс " + ps[0].ToString()); }
            return ps;
        }

        [Description(Desc = "RunCmd(string command) - выполняет указанную команду в консоли cmd.")]
        public static object RunCmd(params object[] ps)
        {
            try
            {
                Process.Start("cmd", ps[0].ToString());
            }
            catch { MessageBox.Show("В RunCmd не передана команда"); return ps; }
            return ps;
        }

        [Description(Desc = "Shutdown() - выключает компьютер.")]
        public static object Shutdown(params object[] ps)
        {
            Process.Start("cmd", "/c shutdown -s -t 00");
            return ps;
        }

        [Description(Desc = "Restart() - перезагрузка компьютера.")]
        public static object Restart(params object[] ps)
        {
            Process.Start("cmd", "/c shutdown -r -t 0");
            return ps;
        }


        [Description(Desc = "SendKeysWait(string Key) - отправляет активному окну кнопку/текст и ждёт окончания обработки.")]
        public static object SendKeysWait(params object[] ps)
        {
            try
            {
                Meths.SetForegroundWindow(Meths.GetForegroundWindow());
                SendKeys.SendWait(ps[0].ToString());
            }
            catch (Exception e){ MessageBox.Show(e.ToString());  return ps; }
            return ps;
        }

        [Description(Desc = "SetCursorPos(int x, int y) - перемещает курсор на заданные координаты.")]
        public static object SetCursorPos(params object[] ps)
        {
            try
            {
                Meths.SetCursorPos((int)ps[0], (int)ps[1]);
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); return ps; }
            return ps;
        }

        [Description(Desc = "MoveCursor(int x, int y, int delay) - перемещает курсор на заданные координаты с промежутком между перемещениями delay.")]
        public static object MoveCursor(params object[] ps)
        {
            try
            {
                
                int endX = (int)ps[0];
                int endY = (int)ps[1];
                int delay = (int)ps[2];
                while (true)
                {
                    var pos = Cursor.Position;
                    if (pos.X != endX)
                    {
                        pos.X += pos.X < endX ? 1 : -1;
                    }
                    if (pos.Y != endY)
                    {
                        pos.Y += pos.Y < endY ? 1 : -1;
                    }
                    Meths.SetCursorPos(pos.X, pos.Y);
                    if (pos.X == endX && pos.Y == endY)
                        break;
                    Thread.Sleep(delay);
                }
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); return ps; }
            return ps;
        }

        [Description(Desc = "MouseEvent(string event) - имитарует событие мыши. Для получения доступных событий использовать GetMouseEventsHelp().")]
        public static object MouseEvent(params object[] ps)
        {
            try
            {
                var Mevent = Enum.Parse(typeof(Meths.MouseEventFlags), ps[0].ToString());
                Meths.mouse_event((uint)(int)Mevent,0,0,0,0);
            }
            catch (NullReferenceException) { MessageBox.Show("В MouseEvent не передано событие " + ps[0]); return ps; }
            catch { MessageBox.Show("В MouseEvent не найдено событие " + ps[0]); return ps; }
            return ps;
        }

        [Description(Desc = "SetClipboardText(string text) - вставляет строку text в буфер обмена Windows.")]
        public static object SetClipboardText(params object[] ps)
        {
            try
            {
                var data = new DataObject();
                Thread thread;
                data.SetData(DataFormats.UnicodeText, true, ps[0]);
                thread = new Thread(() => Clipboard.SetDataObject(data, true));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            catch (IndexOutOfRangeException) { MessageBox.Show("В SetClipboardText не передан текст."); }
            catch (Exception e){ MessageBox.Show(e.Message); }
            return ps;
        }

    }
}
