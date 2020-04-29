using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Runtime.InteropServices;

namespace Binder
{


    public static class LoadFuncsRunScripts
    {
        public enum returnType//тип возвращаемого значения у скрипта
        {
            Void,
            String,
            Double,
            Int,
            Boolean
        }
        //класс медота, тут имя, метод и описание
        [Serializable]
        public class Meth//класс метода
        {
            public MethodInfo m = null;//что вызывать
            public string name = null;//имя для ользователя
            public string desc = null;//описание для пользователя
            public returnType rt;//возвращаемое значение

            public Meth(MethodInfo _m, string _name, string _desc, returnType r)
            {
                m = _m;
                name = _name;
                desc = _desc;
                rt = r;
            }

            public override string ToString()
            {
                return name + " " + desc;
            }
        }


        static bool first = true;//первая подгрузка либы
        static public List<Meth> meths = new List<Meth>();//то, что я и достаю из DLL
        //путь к длл со скриптами
        static readonly string needPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Binder\ScriptsLibrary.dll";
        static public void LoadScripts(string path = "")//метод для подгрузки скриптов
        {

            if (first)//если это первый раз
            {
                if (!File.Exists(needPath))//если либы нет - достаём из ресурсов
                {
                    File.WriteAllBytes(needPath, Properties.Resources.ScriptsLibrary);
                    MessageBox.Show("Нужно обновление библиотеки. Перейдите в Файл->Открыть файл скриптов.", "Обновление");
                }
                Assembly asm = Assembly.LoadFrom(needPath);//загружаем либу
                Obtain(asm);//достаём все скрипты
            }
            else//если не первая подгрузка
            {
                string starterPath = Path.GetTempFileName() + ".exe";//закидываем стартер в темп
                File.WriteAllBytes(starterPath, Properties.Resources.Restarter);//записываем его
                path = path == "" ? needPath : path;//выбираем путь
                Process.Start(starterPath, path + " " + Assembly.GetEntryAssembly().Location);//запускаем стартер
                Environment.Exit(0);//убиваемся
            }

            first = false;//уже не девочка
        }//находим/создаём DLL со скриптами


        static public void Obtain(Assembly asm)//загрузка всех скриптов
        {
            Type BoolScripts;//скрипты, возвращающие BOOL
            Type DoubleScripts;//скрипты, возвращающие Double
            Type IntScripts;//скрипты, возвращающие Int
            Type StringScripts;//скрипты, возвращающие String
            Type VoidScripts;//скрипты, возвращающие Void
            Type[] types;//все скрипты
            meths.Clear();//чистим скрипты (хз зачем)

            //находим типы по каждому типу данных
            BoolScripts = asm.GetTypes().ToList().Find(x => x.Name == "BoolScripts");//получаем тип "Bool"
            DoubleScripts = asm.GetTypes().ToList().Find(x => x.Name == "DoubleScripts");//получаем тип "DoubleScripts"
            IntScripts = asm.GetTypes().ToList().Find(x => x.Name == "IntScripts");//получаем тип "IntScripts"
            StringScripts = asm.GetTypes().ToList().Find(x => x.Name == "StringScripts");//получаем тип "StringScripts"
            VoidScripts = asm.GetTypes().ToList().Find(x => x.Name == "VoidScripts");//получаем тип "VoidScripts"
            types = new Type[5] { BoolScripts, DoubleScripts, IntScripts, StringScripts, VoidScripts };
            //перебираем типы
            foreach (var t in types)
            {
                returnType rt;//выбираем возвращаемое значение
                switch (t.Name)
                {
                    case "BoolScripts":
                        rt = returnType.Boolean;
                        break;
                    case "DoubleScripts":
                        rt = returnType.Double;
                        break;
                    case "IntScripts":
                        rt = returnType.Int;
                        break;
                    case "StringScripts":
                        rt = returnType.String;
                        break;
                    case "VoidScripts":
                        rt = returnType.Void;
                        break;
                    default:
                        continue;
                }
                foreach (var meth in t.GetMethods())//перебираем все методы в типе
                {

                    try
                    {
                        //берём те, где есть мой атрибут с описанием (остальные делают исключения, но они ловятся в catch)
                        CustomAttributeData cad = meth.CustomAttributes.Where(x => x.AttributeType.Name == "Description").First();
                        meths.Add(new Meth(meth, meth.Name, cad.NamedArguments.First().TypedValue.Value.ToString(), rt));
                    }
                    catch { }
                }
            }
            ProgramScripts.AddMeths(meths);//добавляем скрипты, встроенные в программу
        }

        public class Func//функция в скрипте Binder
        {
            public string script;//содержание
            public string name;//имя
            public List<string> prs = new List<string>();//параметры
            public bool asyncFunc = false;//асинхронная или нет

            public Func(string sc, string n, IEnumerable<string> p)
            {
                script = sc;
                name = n;
                prs = p.ToList();
            }
            public Func()
            {
            }

            public override string ToString()
            {
                return name + "(" + string.Join(",", prs) + ")";
            }
        }

        static HashSet<Thread> threads = new HashSet<Thread>();//потоки, выполняющие скрипты
        static public void ExecScript(string sc,  Dictionary<string, object> vars = null, List<Func> fs = null)//выполнить скрипт
        {
            RuntimeFlags flags=RuntimeFlags.None;

            if (sc[0] == '[')
            {
                string f = sc.Substring(1, sc.IndexOf(']') - 1);
                sc = sc.Remove(0, f.Length+2);

                string[] sFlags = GetPars(f).ToArray();
                sFlags.AsParallel().ForAll(s=>s=s.Trim());
                foreach (var flag in sFlags)
                {
                    switch (flag)
                    {
                        case "Block":
                            flags = flags | RuntimeFlags.Block;
                            break;
                        default:
                            MessageBox.Show("Не найден атрибут " + flag + ", выполнение прервано", "Ошибка");
                            return;
                    }
                }
            }

            Thread th = null;//новый поток
            th = new Thread(() =>
            {
                string c = "";
                try
                {
                    List<Func> funcs = fs == null ? new List<Func>() : fs;//функции, созданные в этом потоке
                    Dictionary<string, object> vs = vars == null ? new Dictionary<string, object>() : vars;//переменные в этом потоке

                    foreach (var cmd in GetCommands(sc))//перебираю и запускаю все команды поочереди
                    {
                        c = cmd.Trim();
                        RunCmd(cmd.Trim(), funcs, vs);//выполняю очередную команду
                    }
                }
                catch (Exception e) { MessageBox.Show(e.Message, "Ошибка"); MessageBox.Show(c); }
                threads.Remove(th);//после завершения удаляю поток из списка активных
            });
            th.SetApartmentState(ApartmentState.STA);//хз зачем
            threads.Add(th);//добавляю поток в активные
            th.Start();//запускаю выполнение
            if (flags.HasFlag(RuntimeFlags.Block))//если стоит флаг блокировки, ждём поток
                th.Join();
        }//выполнить скрипт с загруженными командами

        [Flags]
        enum RuntimeFlags
        {
            None = 0,
            Block = 1,
        }

        static List<string> GetCommands(string sc)//получить список команд в скрипте
        {
            List<string> commands = new List<string>();//ответ
            int countBrakets = 0;//счётчик скобок
            for (int i = 0; i < sc.Length; i++)//перебираю всю строку
            {
                if (sc[i] == '{')
                    countBrakets++;
                else if (sc[i] == '}')
                    countBrakets--;
                if (sc[i] == ';' && countBrakets == 0)//если мы не внутри скобки, и нашли ;, то берём команду
                {
                    commands.Add(sc.Substring(0, i).Trim());
                    sc = sc.Remove(0, i + 1);
                    i = 0;
                }
            }
            commands = commands.Where(c => c != "").ToList();//отсеиваем пустые
            commands.AsParallel().ForAll(c => c = c.Trim());//убираем у всех пробелы вначале и в конце
            return commands;//возвращаем
        }
        static readonly Dictionary<string, Dictionary<string, object>> namespaces = new Dictionary<string, Dictionary<string, object>>();
        static bool stop = false;//нужно ли немедленно прекратить выполнение
        static object RunCmd(string cmd, List<Func> funcs, Dictionary<string, object> vars)//для выполнения очередной команды
        {
            //надо помнить, что любая изначально команда, переменная в cmd - это строка, не более
            if (stop)//если надо останавиться - выходим сразу
                return null;
            //чтобы выйти из цикла на любом уровне
            if (cmd == "break")//оператор break
                throw new BreakedExeption("breaked");
            //Вернуть значение из функции
            if (cmd.StartsWith("return "))//оператор return
                throw new ReturnExeption(cmd.Remove(0, 6).Trim());
            //если передана переменная а не команда
            {
                ParType p = GetTypeOfParam(cmd);//получаю тип переданной команды
                if (p != ParType.Method && p != ParType.Invalid)//если это любой тип переменной, возвращаю её значение
                {
                    //получаю тип параметра и его значение в переменные 
                    switch (p)
                    {
                        case ParType.String:
                            return str;
                        case ParType.Double:
                            return doub;
                        case ParType.Int:
                            return integer;
                        case ParType.Boolean:
                            return boolean;
                        default:
                            return null;
                    }
                }
            }
            
            if (cmd.StartsWith("StopAllScripts"))//если это команда для остановки всех скриптов
            {
                //новый поток, который ставит stop=true, а когда всё остановлено stop=false
                Thread th = new Thread(() =>
                {
                    stop = true;
                    while (threads.Count != 0)//ждём, пока есть активные потоки
                    {
                        Thread.Sleep(100);
                    }
                    stop = false;
                });
                th.Start();//запустили поток, и сразу вышли
                return null;
            }

            if (cmd.StartsWith("if"))//если это конструкция if
            {

                List<string> conditions = new List<string>();//условия
                List<string> scripts = new List<string>();//тут все выполняемые части конструкции
                string ifConstruction = cmd;//конструкция
                //разбираться будем по частям, удаляя части из конструкции
                while (ifConstruction != "")//делаем, пока она не пуста
                {
                    if (ifConstruction[0] != '{')//если это условие или else
                    {
                        if (!ifConstruction.Substring(0, ifConstruction.IndexOf("{")).Contains("("))//если это else
                        {
                            ifConstruction = ifConstruction.Substring(ifConstruction.IndexOf("{")).Trim();
                            continue;
                        }
                        string cond = ifConstruction.Substring(0, ifConstruction.IndexOf("{")).Trim();//берём условие
                        cond = cond.Remove(cond.Length - 1, 1);//убираем )
                        cond = cond.Substring(cond.IndexOf("(") + 1);//убираем (
                        conditions.Add(cond);//добавляем это условие
                        ifConstruction = ifConstruction.Substring(ifConstruction.IndexOf("{")).Trim();//вырезаем всё, что только обработали

                    }
                    else//если подошли к действиям
                    {
                        string sc = "";//скрипт
                        int countBrackets = 0;//счётчик скобок
                        int i = 0;
                        for (i = 0; ; i++)//в цикле берём команды, находящиеся до следующего условия
                        {
                            if (ifConstruction[i] == '{')
                                countBrackets++;
                            else if (ifConstruction[i] == '}')
                                countBrackets--;
                            if (countBrackets == 0)
                                break;
                        }
                        sc = ifConstruction.Remove(0, 1).Substring(0, i - 1).Trim();//убираем пробелы
                        ifConstruction = ifConstruction.Substring(i + 1).Trim();//убираем эту часть
                        scripts.Add(sc);//записываем скрипты
                    }

                }
                //разбор завершён
                if (conditions.Count == scripts.Count)//если у нас не написано else, добавляем пустой скрипт для него
                    scripts.Add("");
                for (int i = 0; i < conditions.Count; i++)//перебираем все условия по порядку
                {
                    if ((bool)RunCmd(conditions[i], funcs, vars))//если условие верно, начинаем выполнение соответствующего скрипты
                    {
                        var cmds = GetCommands(scripts[i]);//получаем команды
                        foreach (var c in cmds)//выполняем по 1
                            RunCmd(c, funcs, vars);
                        return null;//выходим
                    }
                }
                var elseCmds = GetCommands(scripts.Last());//если ни обдно из условий неверно, делаем else и выходим
                foreach (var c in elseCmds)
                    RunCmd(c, funcs, vars);
                return null;
            }


            if (cmd.StartsWith("while") || cmd.StartsWith("async while"))//цикл while
            {

                bool async = cmd.StartsWith("async ");//если асинхронно
                if (async)
                {
                    cmd = cmd.Remove(0, 6);//убираем слово "async "
                }
                string param = cmd.Remove(0, 5).Trim();//получаем параметр, убрас слово "while"
                param = param.Remove(0, 1);//убираем (
                param = param.Substring(0, param.IndexOf("{") - 1);//убираем команды
                param = param.Substring(0, param.LastIndexOf(")"));//убираем последнюю скобку

                var r = GetPars(param);//получили параметры
                if (r.Length > 1 || r.Length == 0 || RunCmd(r[0], funcs, vars).GetType() != typeof(bool))//если недостаточно параметров, если их много, или параметр типа не BOOL
                {
                    MessageBox.Show("В while не переданы подходящие аргументы");
                    return null;
                }
                //получаем скрипт
                string sc = cmd.Substring(cmd.IndexOf("{") + 1, cmd.Length - cmd.IndexOf("{") - 2).Trim();
                var cmds = GetCommands(sc);//получаем команды

                if (async)//если асинхронный
                {
                    //новый поток для выполнения
                    Thread th = new Thread(() =>
                    {
                        var obj = RunCmd(r[0], funcs, vars);//проверяем условие
                        bool b = obj != null ? (bool)obj : false;//преобразуем результат выполнения условия в bool
                        while (b)//пока условие истинно
                        {
                            foreach (var c in cmds)//выполняем команды по 1
                            {
                                if (c.Trim() == "break" || stop)//если команда стоп
                                    return;
                                try
                                {
                                    RunCmd(c, funcs, vars);
                                }
                                catch (BreakedExeption) { return; }
                            }
                            obj = RunCmd(r[0], funcs, vars);//снова выполняем условие
                            b = obj != null ? (bool)obj : false;//преобразуем
                        }
                    });
                    th.Start();//запускаем асинхронное выполнение
                }
                else
                {
                    var obj = RunCmd(r[0], funcs, vars);//выполняем условие
                    bool b = obj != null ? (bool)obj : false;//преобразуем в bool
                    while (b)//пока условие истинно
                    {
                        foreach (var c in cmds)//перебираем команда
                        {
                            if (c.Trim() == "break" || stop)//если стоп
                                return null;
                            try
                            {
                                RunCmd(c, funcs, vars);
                            }
                            catch (BreakedExeption) { return null; }
                        }
                        obj = RunCmd(r[0], funcs, vars);//снова проверяем условие
                        b = obj != null ? (bool)obj : false;//преобразуем в bool
                    }
                }

                return null;//выходим
            }

            
            //если это проверить наличие переменной
            if (cmd.StartsWith("CheckVar"))
            {

                cmd = cmd.Remove(0, 8).Trim();//убираем "CheckVar" и пробелы
                cmd = cmd.Remove(0, 1);//убираем (
                cmd = cmd.Remove(cmd.Length - 1, 1);//убираем )

                string[] parameters = GetPars(cmd);//получаем параметры;
                if (parameters.Length == 0)//если параметров 0, выходим
                    return parameters;
                string nameVar = RunCmd(parameters[0], funcs, vars).ToString();//получаем имя переменной
                if (parameters.Length == 1)//если дано только имя
                {
                    return vars.ContainsKey(nameVar);
                }
                else
                {
                    string nameOfSpace = RunCmd(parameters[1], funcs, vars).ToString();
                    if (!namespaces.ContainsKey(nameOfSpace))
                    {
                        return false;
                    }
                    else
                        return namespaces[nameOfSpace].ContainsKey(nameVar);
                }
            }

            //SetVar и GetVar будут реализованые в програмее, а не в библиотеке.
            //если это задать переменную
            if (cmd.StartsWith("SetVar"))
            {

                cmd = cmd.Remove(0, 6).Trim();//убираем "SetVar" и пробелы
                cmd = cmd.Remove(0, 1);//убираем (
                cmd = cmd.Remove(cmd.Length - 1, 1);//убираем )

                string[] parameters = GetPars(cmd);//получаем параметры;
                if (parameters.Length < 2)//если параметров менее 2, выходим
                    return parameters;
                string nameVar = RunCmd(parameters[0], funcs, vars).ToString();//получаем имя переменной
                string param = RunCmd(parameters[1], funcs, vars).ToString();//получаем значение в виде строки
                object value = RunCmd(parameters[1], funcs, vars);//получаем значение
                if (value == null)
                {
                    MessageBox.Show("Значение " + param + " не было сохранено", "Ошибка");
                    return null;
                }
                if (parameters.Length == 2)//если пространство имён не указано
                {
                    vars[nameVar] = value;
                }
                else//если указано
                {
                    string nameOfSpace = RunCmd(parameters[2], funcs, vars).ToString();
                    if (!namespaces.ContainsKey(nameOfSpace))
                        namespaces.Add(nameOfSpace, new Dictionary<string, object>());
                    namespaces[nameOfSpace][nameVar] = value;
                }
                return parameters;
            }

            //если это получить переменную
            if (cmd.StartsWith("GetVar"))
            {
                cmd = cmd.Remove(cmd.Length - 1, 1).Remove(0, 6).Trim();//убираем ),"GetVar" и пробелы
                cmd = cmd.Remove(0, 1);//убираем (
                var parameters = GetPars(cmd);//получаем праметры
                if (parameters.Length == 0)//если параметров нет
                {
                    return parameters;
                }
                try
                {
                    string nameVar = RunCmd(parameters[0], funcs, vars).ToString();
                    
                    return parameters.Length==1? vars[nameVar] : namespaces[RunCmd(parameters[1], funcs, vars).ToString()][nameVar];//пытаемся достать значение переменной по имени
                }
                catch { MessageBox.Show("В GetVar не найдена переменная с именем " + RunCmd(parameters[0], funcs, vars).ToString()); return parameters; }
            }

            //для цикла Repeat
            if (cmd.StartsWith("repeat") || cmd.StartsWith("async repeat") || cmd.StartsWith("allasync repeat"))
            {
                bool asynced = false;//выполнять цикл в отдельном потоке
                bool allasynced = false;//выполнить все сразу
                if (cmd.StartsWith("allasync "))
                {
                    allasynced = true;
                    cmd = cmd.Remove(0, 9);
                }
                else if (cmd.StartsWith("async "))
                {
                    asynced = true;
                    cmd = cmd.Remove(0, 6);
                }
                cmd = cmd.Remove(0, 6).Trim().Remove(0, 1);//убираем "repeat", пробелы и (
                string param = cmd.Substring(0, cmd.IndexOf("{")).Trim();//получаем параметр в виде строки
                param = param.Remove(param.LastIndexOf(")"), param.Length - param.LastIndexOf(")"));//убираю последнюю ) и всё после неё
                int count;//счётчик выполнения
                //получаю значение счётчика
                try
                {
                    count = (int)RunCmd(param, funcs, vars);
                } 
                catch
                {
                    MessageBox.Show("В Repeat передано значение типа, несовместимого с Int", "Ошибка");
                    return null;
                }
                string sc = cmd.Substring(cmd.IndexOf("{"), cmd.Length - cmd.IndexOf("{"));//получаю скрипт
                sc = sc.Remove(sc.Length - 1, 1).Remove(0, 1).Trim();//убираю { и }, пробелы

                //новый поток дял выполнения
                Thread th = new Thread(() =>
                {
                    while (count > 0)//пока не кончится счётчик
                    {
                        if (allasynced)//если полный асинхрон, запускаю отдельно
                        {
                            ExecScript(sc, vars, funcs);
                        }
                        else
                        {
                            foreach (var command in GetCommands(sc))//перебираю и запускаю все поочереди
                            {
                                RunCmd(command.Trim(), funcs, vars);//выполняю очередную команду
                            }
                        }
                        count--;
                    }
                });
                th.Start();//запускаю поток
                if (!asynced)//ждём, если синхронно
                    th.Join();

                return null;
            }

            //если это опеределение функции func name(){}
            if (cmd.StartsWith("func ") || cmd.StartsWith("async func "))
            {
                Func f = new Func();//создаём новую функцию
                if (cmd.StartsWith("async func "))//если это асинхронная
                {
                    f.asyncFunc = true;
                    cmd = cmd.Remove(0, 6);//убираем "async "
                }
                cmd = cmd.Remove(0, 5);//убираем "func "
                f.name = cmd.Substring(0, cmd.IndexOf('('));//достаём имя
                                                            //беру параметры
                f.prs = cmd.Substring(cmd.IndexOf('(') + 1, cmd.IndexOf(')') - cmd.IndexOf('(') - 1).Replace(" ", "").Split(',').ToList();
                cmd = cmd.Remove(0, cmd.IndexOf('{') + 1);//достаю скрипт
                f.script = cmd.Remove(cmd.Length - 1);
                funcs.Add(f);//добавляю новую функцию
                return null;
            }
            //если нашли пользовательскую функцию
            if (funcs.Any(c => c.name == cmd.Substring(0, cmd.IndexOf('('))))
            {
                //находим эту функцию
                Func f = funcs.Find(c => c.name == cmd.Substring(0, cmd.IndexOf('(')));
                StringBuilder scb = new StringBuilder(f.script);//получаем скрипт функции

                //получаем параметры пользовательской функции
                var parameters = GetPars(cmd.Substring(cmd.IndexOf('(') + 1, cmd.LastIndexOf(')') - cmd.IndexOf('(') - 1));

                int count = 0;
                for (int i = 0; i < scb.Length; i++)
                {
                    
                    if (scb[i] == '\"')
                        count++;
                    else if (scb[i] == '(' && count % 2 == 0)
                    {
                        if (scb[i + 1] == ')')
                            continue;
                        else
                        {
                            
                            int lastBracket = 1;
                            for (int sim = i + 1; sim < scb.Length; sim++)
                            {
                                if (scb[sim] == '(')
                                    lastBracket++;
                                else if (scb[sim] == ')')
                                    lastBracket--;
                                if (lastBracket == 0)
                                {
                                    lastBracket = sim;
                                    break;
                                }    
                            }
                            string param = scb.ToString().Substring(i + 1, lastBracket-(i + 1));
                            var cmdPars = GetPars(param);
                            for (int fPar = 0; fPar < f.prs.Count; fPar++)
                            {
                                for (int par = 0; par < cmdPars.Length; par++)
                                {
                                    if (cmdPars[par] == f.prs[fPar])
                                    {
                                        cmdPars[par] = parameters[fPar];
                                    }
                                }
                            }
                            scb.Remove(i + 1, param.Length);
                            scb.Insert(i + 1, string.Join(",", cmdPars));
                        }
                    }
                }
                string sc = scb.ToString();
                if (f.asyncFunc)//если асинхронное
                {
                    ExecScript(sc, vars, funcs);
                    return null;
                }
                else//обычное выполнение
                {
                    var coms = GetCommands(sc);
                    for (int c = 0; c < coms.Count; c++)
                    {
                        try
                        {
                            RunCmd(coms[c], funcs, vars);
                        }
                        catch (ReturnExeption e)
                        {
                            var value = RunCmd(e.Message, funcs, vars);
                            if (value is object[])
                                return (value as object[]).First();
                            else
                                return value.ToString();
                        }
                    }
                }
                return null;
            }



            string script = cmd.Substring(0, cmd.IndexOf('('));//получаю первое ключевое слово
            Meth meth = meths.Find(m => m.name == script);//ищу такой метод
            if (meth == null)//если метод не нашёл - шлю нафиг
            {
                MessageBox.Show(string.Format("Команда не найдена! \"{0}\"", script) + ". Выполнение прервано!");
                return null;
            }
            cmd = cmd.Remove(0, script.Length + 1);//убираю ключевое слово+ первая '('
            cmd = cmd.Remove(cmd.Length - 1).Trim(' ');//убираю последнее ')' и пробелы
            string[] stringPars = GetPars(cmd);//беру оставшиеся параметры
            stringPars.AsParallel().ForAll(p => p = p.Trim(' '));//чищу все параметры от пустышек
            List<object> pars = new List<object>();//создаю объект параметров
            for (int i = 0; i < stringPars.Length; i++)
            {
                ParType p = GetTypeOfParam(stringPars[i].Trim(' '));//получаю тип параметра и его значение в переменные 
                switch (p)
                {
                    case ParType.String:
                        pars.Add(str);
                        break;
                    case ParType.Double:
                        pars.Add(doub);
                        break;
                    case ParType.Int:
                        pars.Add(integer);
                        break;
                    case ParType.Boolean:
                        pars.Add(boolean);
                        break;
                    case ParType.Method:
                        if (stringPars[i] == "")
                            break;
                        var value = RunCmd(stringPars[i], funcs, vars);
                        pars.Add(value is object[]? (value as object[]).First() : value);//если тип переменной-метод, запускаю и сохраняю результат
                        break;
                }
            }


            try
            {
                return meth.m.Invoke(null, new object[] { pars.ToArray() });
            }
            catch (Exception e) { MessageBox.Show(e.Message); return null; }
        }//выполнить одну команду


        static string[] GetPars(string str)//получить массив параметров
        {

            int countBrakeds = 0;//счётчик скобок
            int countMarks = 0;//счётчик кавычек
            List<string> pars = new List<string>();
            str = str + ',';
            for (int i = 0; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case '(':
                        countBrakeds++;
                        break;
                    case ')':
                        countBrakeds--;
                        break;
                    case '\"':
                        countMarks++;
                        break;
                    case ',':
                        if (countBrakeds == 0 && countMarks % 2 == 0)
                        {
                            pars.Add(str.Substring(0, i));
                            str = str.Remove(0, i + 1);
                            i = -1;
                            countBrakeds = 0;
                            countMarks = 0;

                            str = str.Trim(' ');
                        }
                        break;
                }
            }
            if (pars.Count == 0)
                pars.Add(str);
            return pars.ToArray();
        }

        static string str;
        static double doub;
        static int integer;
        static bool boolean;
        static ParType GetTypeOfParam(string par)
        {
            if (par.StartsWith("\"") && par.EndsWith("\""))
            {
                str = par.Remove(par.Length - 1, 1).Remove(0, 1);
                return ParType.String;
            }
            else if (Int32.TryParse(par, out integer))
            {
                return ParType.Int;
            }
            else if (double.TryParse(par.Replace('.', ','), out doub))
            {
                return ParType.Double;
            }
            else if (bool.TryParse(par, out boolean))
            {
                return ParType.Boolean;
            }
            else if (par.Contains("(") && par.EndsWith(")"))
            {
                return ParType.Method;
            }
            else
            {
                return ParType.Invalid;
            }
        }//выяснить какого типа параметр

        enum ParType//возмодные типы параметров в команде
        {
            String,
            Double,
            Int,
            Boolean,
            Method,
            Invalid
        }


    }
}