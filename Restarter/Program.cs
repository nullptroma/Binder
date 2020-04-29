using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Restarter
{
    class Program
    {
        static void Terminate()
        {
            string Body = "Set fso = CreateObject(\"Scripting.FileSystemObject\"): On error resume next: Dim I: I = 0" + Environment.NewLine + "Set File = FSO.GetFile(\"" + Application.ExecutablePath + "\"): Do while I = 0: fso.DeleteFile (\"" + Application.ExecutablePath + "\"): fso.DeleteFile (\"" + Environment.CurrentDirectory + "\\1.vbs\"): " + Environment.NewLine + "If FSO.FileExists(File) = false Then: I = 1: End If: Loop";
            System.IO.File.WriteAllText(Environment.CurrentDirectory + "\\1.vbs", Body, System.Text.Encoding.Default);
            System.Diagnostics.Process.Start(Environment.CurrentDirectory + "\\1.vbs");
        }
        static void Main(string[] args)
        {
            
            Terminate();
            int count = 0;
            while(Process.GetProcessesByName("Binder").Length>0)
            {
                Thread.Sleep(10);
                count++;
                if (count >= 300)
                    Environment.Exit(0);
            }
            string dllPath = args[0];
            string programPath = args[1];
            string needPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Binder\ScriptsLibrary.dll";
            File.Copy(dllPath, needPath, overwrite: true);
            Process.Start(programPath);
            

        }
    }
}
