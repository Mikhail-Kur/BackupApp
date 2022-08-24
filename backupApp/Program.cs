using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace backupApp
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Settings settings =new Settings();
            settings = FindSettings();
            string path = CreateLog();
            if (String.IsNullOrEmpty(settings.urlCopy) == true || String.IsNullOrEmpty(settings.urlOriginal) == true)
            {
                LogText("Отсутвует путь", settings.actionOption, path);
            }
            else if (File.Exists(settings.urlCopy) == true || File.Exists(settings.urlOriginal) == true)
            {
                if (File.Exists(settings.urlCopy) == false)
                {
                    LogText("Папки назначения не существует", settings.actionOption, path);
                }
                else 
                {
                    LogText("Исходной папки не существует", settings.actionOption, path);
                }
            }
            else {
                LogText(Backup(settings, path, settings.actionOption), settings.actionOption, path);
            }

        }
        public static string Backup(Settings settings, string path, string actionOption) {
            int errors=0;
            int files = 0;
            string logTextinfo = string.Format("Режим журналирования: info\nОбработка начата-{0}",DateTime.Now);
            string logTextErrors = "Режим журналирования: error";
            string logTextDebug = "Режим журналирования: debug";
            string message="";

            foreach (string s in Directory.GetFiles(settings.urlOriginal))
            {
                string fileName = Path.GetFileName(s);
                string s1 = settings.urlCopy + "\\" + fileName;
                try
                {
                    File.Copy(s, s1);
                    message = string.Format("\nФайл {0} скопирован успешно", fileName);
                    logTextDebug = logTextDebug.Insert(logTextDebug.Length, message);
                    files++;
                }
                catch (IOException e)
                {
                    message = string.Format("\nФайл {0} уже есть в папке назначения", fileName);
                    logTextDebug = logTextDebug.Insert(logTextDebug.Length, message);
                    message = string.Format("\nобработана ошибка {0}", e.ToString());
                    logTextErrors = logTextErrors.Insert(logTextErrors.Length, message);
                    errors++;
                }
                catch (UnauthorizedAccessException e) {
                    message = string.Format("\nНет доступа к файлу {0}", fileName);
                    logTextDebug = logTextDebug.Insert(logTextDebug.Length, message);
                    message = string.Format("\nобработана ошибка {0}", e.ToString());
                    logTextErrors = logTextErrors.Insert(logTextErrors.Length, message);
                    errors++;
                }
                
            }
            foreach (string s in Directory.GetDirectories(settings.urlOriginal))
            {
                try
                {
                    FileSystem.CopyDirectory(s, settings.urlCopy + "\\" + Path.GetFileName(s));
                    message = string.Format("\nПапка {0} скопирована успешно", s);
                    logTextDebug = logTextDebug.Insert(logTextDebug.Length, message);
                    files++;
                }
                catch (IOException e)
                {
                    message = string.Format("\nПапка {0} уже существует", s);
                    logTextDebug = logTextDebug.Insert(logTextDebug.Length, message);
                    message = string.Format("\nобработана ошибка {0}", e.ToString());
                    logTextErrors = logTextErrors.Insert(logTextErrors.Length, message);
                    errors++;
                }
                catch (UnauthorizedAccessException e)
                {
                    message = string.Format("\nНет доступа к папке {0}", s);
                    logTextDebug = logTextDebug.Insert(logTextDebug.Length, message);
                    message = string.Format("\nобработана ошибка {0}", e.ToString());
                    logTextErrors = logTextErrors.Insert(logTextErrors.Length, message);
                    errors++;
                }

            }
            message = string.Format("\nколличество обработанных ошибок: {0}", errors);
            logTextinfo = logTextinfo.Insert(logTextinfo.Length, message) ;
            switch (actionOption)
            {
                case "info":
                    message = string.Format("\nскопированно элементов {0}", files);
                    logTextinfo = logTextinfo.Insert(logTextinfo.Length, message);
                    message = string.Format("\nрезервная копия создна {0}", DateTime.Now);
                    logTextinfo = logTextinfo.Insert(logTextinfo.Length, message);
                    return logTextinfo;
                case "debug":
                    return logTextDebug;                    
                case "error":
                    return logTextErrors;                  
                default:
                    return "уровень журналирования не указан или не верный выберете из: 'info','debug','error'";   
            }
            
        }
        public static string CreateLog() {
            string logName = string.Format("Log-{0}min{1}sec.txt", DateTime.Now.Minute, DateTime.Now.Second);
            string path = string.Format("..\\{0}", logName);
            var logFile = File.Create(path);
            logFile.Close();
            return path;
        }
        public static void LogText(string text, string actionOption, string path) {   
            File.AppendAllText(path, text);         
        }
        public static Settings FindSettings()
        {
                Settings settings;
                using (StreamReader r = new StreamReader("..\\net5.0\\settings.json"))
                {
                    string json = r.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<Settings>(json);
                }
                return settings;
        }
        public class Settings
        {
            public string urlOriginal;
            public string urlCopy;
            public string actionOption;
        }
    }
}
