using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoHotkey.Interop;

namespace Binder
{
    public class ProgramScripts
    {
        static Assembly ahkAss = Assembly.Load(Properties.Resources.AutoHotkey_Interop);
        static dynamic ahk = ahkAss.GetTypes().ToList().Find(t => t.Name == "AutoHotkeyEngine").GetProperty("Instance").GetValue(null, null);

        public static void AddMeths(List<LoadFuncsRunScripts.Meth> ms)
        {
            ms.Add(new LoadFuncsRunScripts.Meth(typeof(ProgramScripts).GetMethod("KeyDown"),"KeyDown", "KeyDown(string key) - нажимает кнопку key.", LoadFuncsRunScripts.returnType.Void));
            ms.Add(new LoadFuncsRunScripts.Meth(typeof(ProgramScripts).GetMethod("KeyUp"), "KeyUp", "KeyUp(string key) - поднимает кнопку key.", LoadFuncsRunScripts.returnType.Void));
            ms.Add(new LoadFuncsRunScripts.Meth(typeof(ProgramScripts).GetMethod("AHKExecRaw"), "AHKExecRaw", "AHKExecRaw(string script) - выполняет скрипт AHK.", LoadFuncsRunScripts.returnType.Void));
            ms.Add(new LoadFuncsRunScripts.Meth(null, "SetVar", "SetVar(string name, object value, string* namespace) - сохраняет значение value с именем name в пространстве имён namespace, если указано.", LoadFuncsRunScripts.returnType.Void));
            ms.Add(new LoadFuncsRunScripts.Meth(null, "GetVar", "GetVar(string name, string* namespace) -получает значение переменной с именем name в пространстве имён namespace, если указано.", LoadFuncsRunScripts.returnType.Void));
            ms.Add(new LoadFuncsRunScripts.Meth(null, "CheckVar", "CheckVar(string name, string* namespace) - проверяет наличие переменной с именем name в пространстве namespace, если указано.", LoadFuncsRunScripts.returnType.Boolean));
            //ms.Add(new LoadFuncsRunScripts.Meth(typeof(ProgramScripts).GetMethod("EnableBind"), "EnableBind", "EnableBind(string name) - включает бинд name.", LoadFuncsRunScripts.returnType.Void));
            //ms.Add(new LoadFuncsRunScripts.Meth(typeof(ProgramScripts).GetMethod("DisableBind"), "DisableBind", "DisableBind(string name) - выключает бинд name.", LoadFuncsRunScripts.returnType.Void));
        }


        public static object KeyDown(params object[] ps)
        {
            try
            {
                if (ps.Length == 0)
                    return ps;
                Keys key;
                if (Enum.TryParse(ps[0].ToString(), out key))
                {
                    ahk.ExecRaw("Send {" + key.ToString() + " down}");
                }
                else
                {
                    MessageBox.Show("В KeyDown не найдена кнопка " + ps[0].ToString());
                }
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
            return ps;
        }

        public static object KeyUp(params object[] ps)
        {
            if (ps.Length == 0)
                return ps;
            Keys key;
            if (Enum.TryParse(ps[0].ToString(), out key))
            {
                ahk.ExecRaw("Send {" + key.ToString() + " up}");
            }
            else
            {
                MessageBox.Show("В KeyUp не найдена кнопка " + ps[0].ToString());
            }
            return ps;
        }

        public static object AHKExecRaw(params object[] ps)
        {
            try
            {
                ahk.ExecRaw(ps[0].ToString());
            }
            catch (IndexOutOfRangeException) { MessageBox.Show("В AHKExecRaw не передан текст."); }
            return ps;
        }

        public static object EnableBind(params object[] ps)
        {
            //try
            //{
            //    var binds = MainWindow.Binds.Where(b => b.Name.Trim() == ps[0].ToString().Trim());
            //    if(binds.Count() == 0)
            //    {
            //        MessageBox.Show("Бинды с названием "+ ps[0]+ " не найдены");
            //        return ps;
            //    }
            //    binds.AsParallel().ForAll(b => b.Enable = true);
            //    binds.AsParallel().ForAll(b => {
            //        b.EnableButton.Content = "Активен";
            //        b.EnableButton.Background = System.Windows.Media.Brushes.Chartreuse;
            //    });
            //}
            //catch (IndexOutOfRangeException) { MessageBox.Show("В EnableBind не передано название бинда."); }
            return ps;
        }

        public static object DisableBind(params object[] ps)
        {
            //try
            //{
            //    var bnds = MainWindow.Binds.Where(b => b.Name.Trim() == ps[0].ToString().Trim());
            //    foreach (var b in bnds)
            //    {
            //        MessageBox.Show(b.Name, "бинды");
            //    }

            //    if (bnds.Count() == 0)
            //    {
            //        MessageBox.Show("Бинды с названием " + ps[0] + " не найдены");
            //        return ps;
            //    }
            //    bnds.AsParallel().ForAll(b => b.Enable = false);
            //    bnds.AsParallel().ForAll(b =>
            //    {
            //        b.EnableButton.Content = "Неактивен";
            //        b.EnableButton.Background = System.Windows.Media.Brushes.Transparent;
            //    });
            //}
            //catch (IndexOutOfRangeException) { MessageBox.Show("В DisableBind не передано название бинда."); }
            return ps;
        }
    }
}
