using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using LowLevelControls.Natives;

namespace Binder
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static MainWindow()
        {
            Resolver.RegisterDependencyResolver();//для подгружания библиотек
        }

        public class SettingClass
        {
            public bool autoRun = false;//автозапуск программы
            public bool darkTheme = false;//тёмная тема
            public bool AutoRunHided = false;//автозапуск программы в свёрнутом режиме
            public bool haveDefaultBindsPath = false;//открывать ли какое либо сохранение поумолчанию
            public string defaultBindsPath="";//дефолтный путь к бинду


            public static void RegisterAutoRun()//включить автозапуск
            {
                const string applicationName = "Binder";
                const string pathRegistryKeyStartup =
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

                using (RegistryKey registryKeyStartup =
                            Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true))
                {
                    registryKeyStartup.SetValue(
                        applicationName,
                        string.Format("\"{0}\"", System.Reflection.Assembly.GetExecutingAssembly().Location));
                }
            }

            public static void UnRegisterAutoRun()//выключить автозапуск
            {
                const string applicationName = "Binder";
                const string pathRegistryKeyStartup =
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

                using (RegistryKey registryKeyStartup =
                            Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true))
                {
                    registryKeyStartup.DeleteValue(applicationName, false);
                }
            }

            public SettingClass()
            { }
        }

        SettingClass sets = new SettingClass();//настройки программы
        public static readonly List<Bind> Binds = new List<Bind>();//все включенные бинды
        static Bind lastSelectToScripting = null;//выбранный бинд для редактирования
        public static volatile MainWindow main;
        static SolidColorBrush scbSelected = new SolidColorBrush(Color.FromArgb(190, 20, 80, 220));//кисть для выделения
        static SolidColorBrush scbBackScripts = new SolidColorBrush(Color.FromArgb(80, 10, 120, 10));//кисть для заднего фона скриптов

        [Serializable]
        public class Bind
        {
            public Border bind = null;//основной элемент бинда
            public Canvas can = null;//канвас, где все элементы бинда
            public DockPanel NameDock = null;//док, где имя и кнопка редактирования имени
            public Label nameLab = null;//лейбел имени бинда
            public Label script1Lab = null;//лейбел для скрипта 1 бинда
            public Label script2Lab = null;//лейбел для скрипта 2 бинда
            public Button EnableButton = null;//кнопка включения/выключения бинда
            public CheckBox IsToggleCheck = null;//комбобокс с выбором модификатора бинда
            public DockPanel ChooseBindDock = null;//док, для хранения бинда
            public TextBlock keyText = null;//текстблок для кнопки бинда
            public ComboBox comboModifier1 = null;//комбобокс с выбором модификатора бинда1
            public ComboBox comboModifier2 = null;//комбобокс с выбором модификатора бинда2
            public bool TogglePos = false;
            public bool Enable = true;
            public Key[] Keys;//клавиши бинда

            //свойство имени бинда
            public string Name
            {
                get { return name; }
                set 
                {
                    try//при десериализации полей nameLab и scriptLab нету, будет крашить. костыль
                    {
                        nameLab.Content = value;//сохраняем изменения в сам бинд
                    }
                    catch { }
                    name = value;
                }
            }

            //свойство скрипта 1
            public string Script1
            {
                get 
                {
                    return script1; 
                }
                set
                {
                    try//при десериализации полей nameLab и scriptLab нету, будет крашить. костыль
                    {
                        script1Lab.Content = value;//сохраняем в бинд новый скрипт
                    }
                    catch { }
                    script1 = value;
                }
            }

            public string Script2
            {
                get
                {
                    return script2;
                }
                set
                {
                    try//при десериализации полей nameLab и scriptLab нету, будет крашить. костыль
                    {
                        script2Lab.Content = value;//сохраняем в бинд новый скрипт
                    }
                    catch { }
                    script2 = value;
                }
            }

            public bool IsToggle
            {
                get { return isToggle; }
                set 
                {
                    isToggle = value;
                    try
                    {
                        IsToggleCheck.IsChecked = value;
                        UpdateScriptBox();
                    }
                    catch { }
                }
            }

           
            string name = "";//имя
            string script1 = "";//скрипт №1
            string script2 = "";//скрипт №2
            bool isToggle = false;//этот бинд - переключатель?

            public Bind()//пустой конструктор для XML сериализации
            {
                
            }


            public void Start()
            {
                name = nameLab.Content.ToString();
                script1 = script1Lab.Content.ToString();
                script2 = script2Lab.Content.ToString();
            }
            
            public void Select()//выделить этот бинд
            {
                if (Binds.Count != 0)
                    Binds.ForEach(b => b.UnSelect());
                bind.BorderBrush = scbSelected;//делаем синюю границу
                lastSelectToScripting = this;
                UpdateScriptBox();
            }


            public void UnSelect()//убрать выделение
            {
                if (lastSelectToScripting != this)
                    return;
                bind.BorderBrush = Brushes.Gray;//возвращаем границу
            }

            
            public void UpdateScriptBox()
            {
                if (isToggle)
                {
                    main.ScriptBox.Visibility = Visibility.Collapsed;
                    main.ScriptBoxOn.Text = script1;
                    main.EditScriptsOnOff.Visibility = Visibility.Visible;
                    main.ScriptBoxOn.Visibility = Visibility.Visible;
                    main.ScriptBoxOff.Visibility = Visibility.Visible;
                    main.ScriptBoxOn.Text = script1;
                    main.ScriptBoxOff.Text = script2;
                    UpdateBacks();
                }
                else
                {
                    main.EditScriptsOnOff.Visibility = Visibility.Collapsed;
                    main.ScriptBoxOn.Visibility = Visibility.Collapsed;
                    main.ScriptBoxOff.Visibility = Visibility.Collapsed;
                    main.ScriptBox.Visibility = Visibility.Visible;
                    main.ScriptBox.Text = script1;
                }
            }

            public void UpdateBacks()
            {
                main.ScriptBoxOff.Background = Brushes.Transparent;
                main.ScriptBoxOn.Background = Brushes.Transparent;
                if(isToggle&& lastSelectToScripting == this)
                    (!TogglePos ? main.ScriptBoxOff : main.ScriptBoxOn).Background = scbBackScripts;
                main.ScriptBoxOn.UpdateDefaultStyle();
                main.ScriptBoxOff.UpdateDefaultStyle();
            }
        }

        //добавляет бинд
        void AddBind(string nameBind,bool toggle, string scr1, string scr2, Key[] keys, bool enable)//доабвляет бинд на панель
        {
            Button editNameBut = new Button()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = "Ред.",
                Width = 40,
                Height = 20,
                Margin = new Thickness(0, 3, 0, 0)
            };//кнопка редактирования имени
            Label nameLab = new Label()
            { 
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Name = "Name",
                Content = nameBind
            };//название бинда
            TextBox nameBox = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Visibility = Visibility.Hidden,//изначально скрываем
                Height = 20,
                Margin = new Thickness(3, 3, 3, 0)
            };//редактор имени бинда
            
            DockPanel DockName = new DockPanel()
            {
                Name = "DockForName",
                Height = 30,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };//док для имени и редактирования имени
            DockName.Children.Add(nameLab);//добавляю в док имя
            DockName.Children.Add(nameBox);//добавляю редактор имени 
            DockName.Children.Add(editNameBut);//добавляю кнопку редактирования

            Label script1 = new Label()
            {
                Name = "Script1",
                Visibility = Visibility.Collapsed,//чтобы не было видно, его не надо
                Content = scr1
            };//для хранения скрипта 1 у бинда
            Label script2 = new Label()
            {
                Name = "Script2",
                Visibility = Visibility.Collapsed,//чтобы не было видно, его не надо
                Content = scr2
            };//для хранения скрипта 2 у бинда

            CheckBox IsToggle = new CheckBox()
            {
                FlowDirection = FlowDirection.RightToLeft,
                Name = "IsToggle",
                Content="Бинд-переключатель",
                IsChecked = toggle,
            };//галочка для бинда-переключателя

            Button deleteBut = new Button()
            {
                Height = 16,
                Width = 16,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
            };//кнопка удаления скрипта
            //картинка на кнопку
            deleteBut.Content = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/Pictures/удаление.png")) };

            Button enableBut = new Button()
            {
                Height = 16,
                Content = enable ? "Активен" : "Неактивен",
                VerticalContentAlignment = VerticalAlignment.Top,
                FontSize = 10,
                Background = enable ? Brushes.Chartreuse : Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
            };//кнопка удаления скрипта

            //элементы для выбора горячих клавиш
            TextBlock textBind = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = "Бинд:",
                Margin = new Thickness(1)
            };//текст "Бинд"
            TextBlock keyBind = new TextBlock()
            {
                Name="textBind",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Text = keys.Length==0 ? "<клавиша>" : string.Join(" + ", keys),
                Margin = new Thickness(3,1,1,1)
            };//поле для нажатия кнопки



            //док, для выбора имени
            DockPanel chooseBind = new DockPanel()
            {
                Name="ChooseBind",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Height = 20
            };
            chooseBind.Children.Add(textBind);
            chooseBind.Children.Add(keyBind);


            Canvas can = new Canvas()//док, где всё находится
            {
                Height = 75
            };

            can.Children.Add(chooseBind);
            Canvas.SetLeft(chooseBind, 1);
            Canvas.SetBottom(chooseBind, 1);

            can.Children.Add(DockName);
            Canvas.SetLeft(editNameBut, 100);

            can.Children.Add(deleteBut);//впихиваю кнопку удаления
            Canvas.SetRight(deleteBut, 2);
            Canvas.SetTop(deleteBut, 2);

            can.Children.Add(enableBut);//впихиваю кнопку удаления
            Canvas.SetRight(enableBut, 30);
            Canvas.SetTop(enableBut, 2);

            can.Children.Add(IsToggle);//впихиваю переключатель
            Canvas.SetBottom(IsToggle,2);
            Canvas.SetRight(IsToggle,2);

            can.Children.Add(script1);//впихиваю скрипт
            can.Children.Add(script2);//впихиваю скрипт
            
            

            Border bord = new Border()//граница
            {
                Background = new SolidColorBrush(Color.FromArgb(15, 20, 20, 30)),//задний фон границы
                BorderBrush = Brushes.Gray,//чем рисуем границу\
                BorderThickness = new Thickness(3, 3, 3, 3),//толщина границы
                Margin = new Thickness(3, 3, 3, 3),//отступ
                Child = can,
            };

            Bind thisBind = new Bind();//представляем текущий бинд в добном виде

            thisBind.Keys = keys;

            thisBind.bind = bord;
            thisBind.can = can;
            thisBind.ChooseBindDock = chooseBind;
            thisBind.EnableButton = enableBut;
            thisBind.IsToggleCheck = IsToggle;
            thisBind.keyText = keyBind;
            thisBind.NameDock = DockName;
            thisBind.nameLab = nameLab;
            thisBind.script1Lab = script1;
            thisBind.script2Lab = script2;
            thisBind.Enable = enable;
            thisBind.Start();


            //обработчик события на новую клавишу для бинда
            keyBind.MouseDown += (object sender, MouseButtonEventArgs e) => 
            {
                Hook.Block();
                Hook.UnHook();
                keyBind.Text = "<нажмите сочетание>";
                HashSet<Key> inputKeys = new HashSet<Key>();
                void KeyBindDown(object sender2, GlobalHook.MyEventArgs e2)
                {
                    var key = e2.e.Key == Key.System ? e2.e.SystemKey : e2.e.Key;
                    if (!inputKeys.Contains(key))
                    {
                        inputKeys.Add(key);
                        keyBind.Text = string.Join(" + ", inputKeys);
                    }
                };
                void KeyBindUp(object sender2, GlobalHook.MyEventArgs e2)
                {
                    Hook.gh.KeyDown -= KeyBindDown;
                    Hook.gh.KeyUp -= KeyBindUp;
                    thisBind.Keys = inputKeys.ToArray();
                    Hook.UnBlock();
                    Hook.SetHook();
                };

                Hook.gh.KeyDown += KeyBindDown;
                Hook.gh.KeyUp += KeyBindUp;
            };

            nameBox.KeyUp += (object sender, KeyEventArgs e) =>//проверяем, если энтер или ескейп то выйти
            {
                if (e.Key == Key.Enter || e.Key == Key.Escape)
                {
                    nameLab.Content = nameBox.Text;
                    thisBind.Name = nameBox.Text;
                    nameBox.Visibility = Visibility.Collapsed;
                    nameLab.Visibility = Visibility.Visible;
                }
            };
            nameBox.LostFocus += (object sender, RoutedEventArgs e) =>
            {
                nameLab.Content = nameBox.Text;
                thisBind.Name = nameBox.Text;
                nameBox.Visibility = Visibility.Collapsed;
                nameLab.Visibility = Visibility.Visible;
            };//если потеряли фокус, перенести текст


            //обработчик нажатия на кнопку редактирования
            editNameBut.Click += (object sender, RoutedEventArgs e) =>
            {
                nameBox.Visibility = Visibility.Visible;
                nameLab.Visibility = Visibility.Collapsed;
                nameBox.Focus();
            };

            //обработчик на кнопку удаления
            deleteBut.Click += (object sender, RoutedEventArgs e) =>
            {
                if (MessageBox.Show("Удалить бинд \"" + nameLab.Content.ToString() + "\"?","Вы уверены?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    StackBinds.Children.Remove(bord);
                    Binds.Remove(thisBind);//удаляем наш бинд из глобального списка
                    if (lastSelectToScripting == thisBind)
                        ScriptBox.Text = "";
                }
            };

            //обработчик на нажатие на бинд в любом месте
            bord.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => //выделить синим
            {
                
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    thisBind.Select();
                }
            };

            enableBut.Click += (object seneder, RoutedEventArgs ea) =>
            {
                thisBind.Enable = !thisBind.Enable;
                enableBut.Content = thisBind.Enable ? "Активен" : "Неактивен";
                enableBut.Background = thisBind.Enable ? Brushes.Chartreuse : Brushes.Transparent;
            };

            IsToggle.Click += (object seneder, RoutedEventArgs ea) =>
            {
                thisBind.IsToggle = IsToggle.IsChecked.Value;
                thisBind.Select();
            };
            thisBind.IsToggle = toggle;


            Binds.Add(thisBind);//записываем этот бинд в список всех биндов
            StackBinds.Children.Add(bord);//впихиваю границу в стак
        }

        //сохранение скрипта в scriptBox
        private void SaveScript_Click(object sender, RoutedEventArgs e)
        {
            ScriptBoxOn.Text = FormateScripts(ScriptBoxOn.Text);
            ScriptBoxOff.Text = FormateScripts(ScriptBoxOff.Text);
            ScriptBox.Text = FormateScripts(ScriptBox.Text);
            if (lastSelectToScripting.IsToggle)
            {
                lastSelectToScripting.Script1 = ScriptBoxOn.Text;
                lastSelectToScripting.Script2 = ScriptBoxOff.Text;
            }
            else
                lastSelectToScripting.Script1 = ScriptBox.Text;

            MessageBox.Show("Сохранено");
        }

        private string FormateScripts(string sb)
        {
            int count = 0;
            var strs = sb.Split('\n');
            for (int i = 0; i < strs.Length; i++)
            {
                if (strs[i].Length == 0)
                    continue;
                strs[i] = strs[i].Trim(' ');
                if (strs[i][0] == '{')
                {
                    if (count > 0)
                        strs[i] = string.Join("", Enumerable.Repeat("    ", count)) + strs[i];
                    count++;
                    continue;
                }
                else if (strs[i][0] == '}')
                {
                    count--;
                }
                if (count > 0)
                {
                    strs[i] = string.Join("", Enumerable.Repeat("    ", count)) + strs[i];
                }
                    
            }

            return string.Join("\n", strs);
        }

        //тут все ссылочные переменные, на которые надо поставить игнор
        //public Border bind = null;//основной элемент бинда
        //public Canvas can = null;//канвас, где все элементы бинда
        //public DockPanel NameDock = null;//док, где имя и кнопка редактирования имени
        //public Label nameLab = null;//лейбел имени бинда
        //public Label script1Lab = null;//лейбел для скрипта 1 бинда
        //public Label script2Lab = null;//лейбел для скрипта 2 бинда
        //public Button EnableButton = null;//кнопка включения/выключения бинда
        //public CheckBox IsToggleCheck = null;//комбобокс с выбором модификатора бинда
        //public DockPanel ChooseBindDock = null;//док, для хранения бинда
        //public TextBlock keyText = null;//текстблок для кнопки бинда
        //public ComboBox comboModifier1 = null;//комбобокс с выбором модификатора бинда1
        //public ComboBox comboModifier2 = null;//комбобокс с выбором модификатора бинда2

        public MainWindow()//конструктор этого окна
        {
            //подготавливаем сериализер для биндов
            {
                var overrides = new XmlAttributeOverrides();
                var ignore = new XmlAttributes { XmlIgnore = true };
                overrides.Add<Bind>(m => m.bind, ignore);
                overrides.Add<Bind>(m => m.can, ignore);
                overrides.Add<Bind>(m => m.ChooseBindDock, ignore);
                overrides.Add<Bind>(m => m.comboModifier1, ignore);
                overrides.Add<Bind>(m => m.comboModifier2, ignore);
                overrides.Add<Bind>(m => m.EnableButton, ignore);
                overrides.Add<Bind>(m => m.IsToggleCheck, ignore);
                overrides.Add<Bind>(m => m.keyText, ignore);
                overrides.Add<Bind>(m => m.NameDock, ignore);
                overrides.Add<Bind>(m => m.nameLab, ignore);
                overrides.Add<Bind>(m => m.script1Lab, ignore);
                overrides.Add<Bind>(m => m.script2Lab, ignore);
                Type t = typeof(Bind[]);
                xsBinds = new XmlSerializer(t, overrides);
            }
            
            if (Application.ResourceAssembly.Location.Split('\\').Last()!="Binder.exe")//возвращаем наше имя
            {
                File.Move(Application.ResourceAssembly.Location, Application.ResourceAssembly.Location.Replace(Application.ResourceAssembly.Location.Split('\\').Last(), "Binder.exe"));
            }
            if (Process.GetProcessesByName("Binder").Length > 1)
            {
                Application.Current.Shutdown();
            }
            
            
            InitializeComponent();
            OpenOrCreateSets();//загружаем настройки
            //применяем настройки
            if (sets.haveDefaultBindsPath)
            {
                try
                { OpenInPath(sets.defaultBindsPath); }
                catch//если не удалось открыть или найти файл сохранки то убираем это
                {
                    MessageBox.Show("Файл " + sets.defaultBindsPath +" не найден!");
                    sets.defaultBindsPath = "";
                    sets.haveDefaultBindsPath = false;
                }
            }
            SettingClass.UnRegisterAutoRun();
            if (sets.autoRun)
            {
                SettingClass.RegisterAutoRun();
            }
            
            Hook.SetHook();//запускаем хукер
            
            ShowWindow();
            Show();
            if (sets.AutoRunHided)
                HideWindow();
            LoadFuncsRunScripts.LoadScripts();
            main = this;
            MouseHook.MouseMove += MousePos;//ставим обработчик
            //MouseHook.LocalHook = false;
            
        }

        //кнопка добавляения бинда
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddBind("Бинд №" + StackBinds.Children.Count, false, "","", new Key[0], true);
            Binds.Last().Select();
        }
        
        

        

        //загрузка/сохранение биндов
        string lastOpenPath = "";
        //открыть бинды
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                FileName = lastOpenPath,
                DefaultExt = ".xml",
                Filter = "XML Files (.xml)|*.xml"
            };//диалог открытия файла
            if (ofd.ShowDialog().Value)//если открыли
            {
                OpenInPath(ofd.FileName);
            }
        }

        //сохранение биндов в файл
        private void SaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            //ищем полный путь к файлу для сохранениня
            SaveFileDialog sfd = new SaveFileDialog()
            {
                FileName = "Binds",
                DefaultExt = ".xml",
                Filter = "XML Files (.xml)|*.xml"
            };
            if (sfd.ShowDialog().Value)//Если путь есть
            {
                SaveInPath(sfd.FileName);
                lastOpenPath = sfd.FileName;
            }

        }
        XmlSerializer xsBinds = null;
        //сохранить бинды в последний файл биндов
        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (lastOpenPath == "")
                SaveAsFile_Click(null, null);
            else
                SaveInPath(lastOpenPath);
        }
        //сохранить бинды по какому то пути
        private void SaveInPath(string path)
        {
            Bind[] bds = Binds.ToArray();//массив биндов
            using (FileStream fs = File.Create(path))
            {
                
                xsBinds.Serialize(fs, bds);//сохраняем
            }
            lastOpenPath = path;
        }
        //открыть бинды по пути
        private void OpenInPath(string path)
        {
            StackBinds.Children.Clear();//чистим текущие бинды
            Binds.Clear();//чистим текущие бинды
            lastSelectToScripting = null;//убираем последний использованный бинд
            ScriptBox.Text = "";//чистим поле для скриптов

            Bind[] binds = null;
            using (FileStream fs = File.OpenRead(path))
            {
                binds = (Bind[])xsBinds.Deserialize(fs);//открываем
            }

            foreach (var b in binds)
            {
                AddBind(b.Name,b.IsToggle, b.Script1, b.Script2, b.Keys, b.Enable);
            }

            lastOpenPath = path;
        }

        HelpWindow hw = null;
        bool HaveHW = false;
        //кнопка помощи, откроет окно со всеми скриптами
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            if (HaveHW)
                return;
            hw = new HelpWindow();
            hw.Closed+=(object seneder, EventArgs ea)=>
            {
                HaveHW = false;
            };

            hw.VoidHelps.Text = "Скрипты, не возвращающие значения. По сути они возвращают, но это возвращаемое значение-просто аргументы, переданные в функцию. Их можно передать в аргументы другого скрпта, при этом выполнится их действие и сам скрипт с одними и теми же аргументами. Скрипты:";
            hw.VoidHelps.Text += Environment.NewLine;

            hw.StringHelps.Text = "Скрипты, возвращающие значения типа String, возвращают строку, могут применяться в качестве параметров к скриптам с параметрами типа String. Скрипты:";
            hw.StringHelps.Text += Environment.NewLine;
            
            hw.DoubleHelps.Text = "Скрипты, возвращающие значения типа Double, возвращают дробное число, могут применяться в качестве параметров к скриптам с параметрами типа Double. Скрипты:";
            hw.DoubleHelps.Text += Environment.NewLine;
            
            hw.IntHelps.Text = "Скрипты, возвращающие значения типа Integer, возвращают целое число, могут применяться в качестве параметров к скриптам с параметрами типа Integer. Скрипты:";
            hw.IntHelps.Text += Environment.NewLine;
            
            hw.BooleanHelps.Text = "Скрипты, возвращающие значения типа Boolean, возвращают булево значение (true/false), могут применяться в качестве параметров к скриптам с параметрами типа Boolean. Скрипты:";
            hw.BooleanHelps.Text += Environment.NewLine;

            hw.AtrsHelps.Text = "Чтобы использовать атрибуты, нужна написать их вначале скрипта в квадратных скобках [] через запятую." + Environment.NewLine + "Поддерживаемые атрибуты:" + Environment.NewLine;
            hw.AtrsHelps.Text += "*Block - блокирует передачу клавиши другим приложениям до завершения основного потока бинда." + Environment.NewLine + "*Exclusive - делает клавиши бинда эксклюзивными.";

            hw.img.Source = new BitmapImage(new Uri("pack://application:,,,/Pictures/помощь.png"));

            foreach (var m in LoadFuncsRunScripts.meths)
            {
                switch (m.rt)
                {
                    case LoadFuncsRunScripts.returnType.Void:
                        hw.VoidHelps.Text += "*" + m.desc + Environment.NewLine;
                        break;
                    case LoadFuncsRunScripts.returnType.String:
                        hw.StringHelps.Text += "*" + m.desc + Environment.NewLine;
                        break;
                    case LoadFuncsRunScripts.returnType.Double:
                        hw.DoubleHelps.Text += "*" + m.desc + Environment.NewLine;
                        break;
                    case LoadFuncsRunScripts.returnType.Int:
                        hw.IntHelps.Text += "*" + m.desc + Environment.NewLine;
                        break;
                    case LoadFuncsRunScripts.returnType.Boolean:
                        hw.BooleanHelps.Text += "*" + m.desc + Environment.NewLine;
                        break;
                    default:

                        break;
                }
            }

            hw.Show();
            hw.Activate();
            HaveHW = true;
        }


        SettingsWindow stw = null;
        //настройки программы
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            stw = new SettingsWindow();//создаём новое окно настроек
            //загружаем все настройки в окно настроек
            stw.SelectAutoBindsPath.IsEnabled = sets.haveDefaultBindsPath;//включаем выбор бинда поумолчанию, если надо
            stw.DefaultBindsPath.IsEnabled = sets.haveDefaultBindsPath;//включаем выбор бинда поумолчанию, если надо
            stw.AutoOpenBinds.IsChecked = sets.haveDefaultBindsPath;//включаем выбор бинда поумолчанию, если надо
            stw.DefaultBindsPath.Text = sets.defaultBindsPath;//вставляем путь до бинда по стандарту
            stw.AutorunCheckBox.IsChecked = sets.autoRun;//включаем/выключаем автозапуск
            stw.RunHided.IsChecked = sets.AutoRunHided;//включаем/выключаем скрытый запуск


            //при нажатии не чекбокс с выбором биндов поумолчанию
            stw.AutoOpenBinds.Click += (object seneder, RoutedEventArgs eBinds) =>
            {
                stw.SelectAutoBindsPath.IsEnabled = stw.AutoOpenBinds.IsChecked.Value;
                stw.DefaultBindsPath.IsEnabled = stw.AutoOpenBinds.IsChecked.Value;
                sets.haveDefaultBindsPath = stw.AutoOpenBinds.IsChecked.Value;
            };

            //при нажатии на кнопку выбора пути к файлу
            stw.SelectAutoBindsPath.Click += (object seneder, RoutedEventArgs eBinds) =>
            {
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    FileName = lastOpenPath,
                    DefaultExt = ".xml",
                    Filter = "XML Files (.xml)|*.xml"
                };//диалог открытия файла
                if (ofd.ShowDialog().Value)//если открыли
                {
                    sets.defaultBindsPath = ofd.FileName;
                    stw.DefaultBindsPath.Text = ofd.FileName;
                }
            };

            //при изменении значения автозапуска
            stw.AutorunCheckBox.Click += (object seneder, RoutedEventArgs eBinds) =>
            {
                sets.autoRun = stw.AutorunCheckBox.IsChecked.Value;
                SettingClass.UnRegisterAutoRun();
                if (sets.autoRun)
                    SettingClass.RegisterAutoRun();
            };

            //при изменении скрытого запуска
            stw.RunHided.Click += (object seneder, RoutedEventArgs eBinds) =>
            {
                sets.AutoRunHided = stw.RunHided.IsChecked.Value;
            };



            stw.Closing+=(object seneder, System.ComponentModel.CancelEventArgs eBinds) =>
            {
                SaveSets();
            };

            stw.ShowDialog();
            stw.Activate();
        }



        readonly XmlSerializer xsSets = new XmlSerializer(typeof(SettingClass));//сериализер для настроек
        //открывает настройки, если нету - создаёт
        private void OpenOrCreateSets()
        {
            try//пытаемся загрузить настройки
            {
                using (FileStream fs = File.OpenRead(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Binder\settings.xml"))
                {
                    sets = (SettingClass)xsSets.Deserialize(fs);
                }
            }
            catch { SaveSets(); }//если не получилось то сохраняем стандартные
            
        }

        //сохранить настройки программы
        private void SaveSets()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Binder"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Binder");
            }
            using (FileStream fs = File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Binder\settings.xml"))
            {
                xsSets.Serialize(fs, sets);//сохраняем
            }
        }

        //далее управление отображением окна:
        //при закрытии окна 
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;//отменяем закрытие
            HideWindow();//сворачиваемся
        }
        //прри двойном клике на значок в трее
        private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            HideWindow();
            ShowWindow();
        }
        //функция скрытия окна
        void HideWindow()
        {
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false; 
            this.Visibility = Visibility.Hidden;
            MouseHook.UnInstallHook();
            
        }
        //функция показа окна
        void ShowWindow()
        {
            this.Visibility = Visibility.Visible;
            ShowInTaskbar = true;
            WindowState = WindowState.Normal;
            MouseHook.InstallHook();
            
        }

        private void MousePos(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            CursorPos.Content = "Координаты курсора " + e.Location;
        }

        //при нажатии на кнопку скрытия в трее
        private void HideWindowBut(object sender, RoutedEventArgs e)
        {
            HideWindow();
        }
        //при нажатии на кнопку отображения в трее
        private void ShowWindowBut(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }
        //кнопка выход в трее
        private void ExitBut_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            SaveSets();
        }

        //открыть новые скрипты
        private void OpenScripts_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                DefaultExt = ".dll",
                Filter = "DLL Files (.dll)|*.dll"
            };//диалог открытия файла
            if (ofd.ShowDialog().Value)//если открыли
            {
                LoadFuncsRunScripts.LoadScripts(ofd.FileName);
            }

        }

        private void ScriptBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Hook.SetHook();
        }

        private void ScriptBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Hook.UnHook();
        }

        RecordWindow rw;
        bool haveRW = false;
        //кнопка записи
        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (haveRW)
            {
                rw.Focus();
                return;
            }    
            rw =new RecordWindow();
            rw.Closed += (sender2, e2) => { haveRW = false; };
            rw.Show();
            haveRW = true;
        }
    }
}
