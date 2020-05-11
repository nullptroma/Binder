using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Win32;

namespace Binder
{
    public static class Hook//класс хука для биндов
    {
        public static readonly GlobalHook gh = new GlobalHook();//хукер

        static HashSet<Key> hs = new HashSet<Key>();//словарь нажатых клавиш
        static void KeyDown(object sender, GlobalHook.MyEventArgs e)//любая кнопка нажата
        {
            if (!hs.Contains(e.e.Key))//если такая кнопка не нажата, то записываем её и продолжаем
                hs.Add(e.e.Key);
            else//если кнопка уже была нажата - выходим
                return;
            var bnds = MainWindow.Binds.Where(b => b.Keys.Contains(e.e.Key==Key.System?e.e.SystemKey:e.e.Key));//ищем бинды, где кнопка соответствует
            foreach (var b in bnds)//береьираем эти бинды
            {
                //если нажаты все необходимые кнопки
                if (b.Enable && b.Keys.All(k=>hs.Contains(k)))
                {
                    try
                    {
                        string scr = "";
                        if (b.IsToggle)//Если это бинд переключатель
                        {
                            scr = b.TogglePos ? b.Script1 : b.Script2;//выбираем скрипт
                            b.TogglePos = !b.TogglePos;//переключаем
                            b.UpdateBacks();//обновляем задний фон для его скриптов
                        }
                        else//обычный бинд
                            scr = b.Script1;//просто берём и выполняем
                        int code = LoadFuncsRunScripts.ExecScript(scr);//выполняем скрипт
                        switch (code)
                        {
                            case 1://1 - эксклюзивный бинд
                                e.Handled = true;
                                break;
                        }
                    }
                    catch (Exception ex)//логгер ошибок
                    {
                        string crashLog = string.Join(null, Enumerable.Repeat(Environment.NewLine, 5));
                        crashLog += DateTime.Now.ToString() + Environment.NewLine + ex.ToString();
                        File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Binder\crashLogs.txt", crashLog);
                        MessageBox.Show("Во время выполнения произошла ошибка, откройте файл лога для большей информации", "Ошибка");
                    }
                }
            }

        }

        static void KeyUp(object sender, GlobalHook.MyEventArgs e)//кнопка поднята
        {
            hs.Remove(e.e.Key);//удаляем её из слоавря
        }

        public static void Block()
        {
            gh.block = true;
        }

        public static void UnBlock()
        {
            gh.block = false;
        }

        static public void SetHook()//установить хук
        {
            gh.KeyDown += KeyDown;
            gh.KeyUp += KeyUp;
        }

        static public void UnHook()//убрать хук
        {
            gh.KeyDown -= KeyDown;
            gh.KeyUp -= KeyUp;
        }


        static Hook()
        {
            
        }
    }
}
