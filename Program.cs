using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Runtime.InteropServices;

namespace runasdate
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMiliseconds;
        }
        
        // 用于设置系统时间
        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime sysTime);
        // 用于获得系统时间
        [DllImport("Kernel32.dll")]
        public static extern void GetLocalTime(ref SystemTime sysTime);
        [DllImport("shell32.dll")]
        public static extern int ShellExecute(IntPtr hwnd, StringBuilder lpszOp, StringBuilder lpszFile, StringBuilder lpszParams, StringBuilder lpszDir, int FsShowCmd);

        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]//返回0表示失败，非0为成功
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        static string iniPath = "./runasdate.ini";
        static string GetConfigStringValue(string key)
        {
            StringBuilder sb = new StringBuilder(1024);
            GetPrivateProfileString("main", key, "", sb, 1024, iniPath);
            return sb.ToString();
        }
        static int GetConfigValue(string key)
        {
            int ret = 0;
            try
            {
                ret = Convert.ToInt32(GetConfigStringValue(key));
            }
            catch (Exception)
            { }
            return ret;
        }
        static void SetConfigValue(string key, int v)
        {
            int a = (int)WritePrivateProfileString("main", key, v.ToString(), iniPath);
            Console.WriteLine("set init {0}", a);
        }

        static void Main(string[] args)
        {
            SystemTime systemTime = new SystemTime();
            GetLocalTime(ref systemTime); // 获得系统的时间并存在SystemTime结构体中
            //Console.WriteLine(string.Format("{0}/{1}/{2} {3}/{4}/{5}/{6}", 
            //    systemTime.wYear, systemTime.wMonth, systemTime.wDay,
            //    systemTime.wHour, systemTime.wMinute, systemTime.wSecond, systemTime.wMiliseconds));

            string exe = GetConfigStringValue("exe");
            int init = GetConfigValue("init");
            int year = GetConfigValue("year");
            int month = GetConfigValue("month");
            int day = GetConfigValue("day");
            int sleepSecond = GetConfigValue("sleepSecond");

            if (string.IsNullOrEmpty(exe))
            {
                exe = @"E:\\迅雷下载\\Sfupersocks5cap_3.5.0_XiaZaiZhiJia\\SuperSocks5Cap_RunAsAdmin.exe";
            }
            if (sleepSecond <= 0)
            {
                sleepSecond = 10;
            }

            SystemTime modTime = systemTime;
            if (init == 0)
            {
                init = 1;
                year = systemTime.wYear;
                month = systemTime.wMonth;
                day = systemTime.wDay;
                SetConfigValue("init", init);
                SetConfigValue("year", year);
                SetConfigValue("month", month);
                SetConfigValue("day", day);
            }
            modTime.wYear = (ushort)year;
            modTime.wMonth = (ushort)month;
            modTime.wDay = (ushort)day;
            SetLocalTime(ref modTime);
            Console.WriteLine(string.Format("回退时间到 {0}/{1}/{2} ",
                    modTime.wYear, modTime.wMonth, modTime.wDay));

            Console.WriteLine("启动{0}", exe);

            ShellExecute(IntPtr.Zero, new StringBuilder("Open"), new StringBuilder(exe), new StringBuilder(""), new StringBuilder(""), 1);
            //ShellExecute(IntPtr.Zero, new StringBuilder("Open"), new StringBuilder(@"C:\Program Files\Notepad++\notepad++.exe"), new StringBuilder(""), new StringBuilder(""), 1);
            //ShellExecute(IntPtr.Zero, new StringBuilder("Open"), new StringBuilder(@"SuperSocks5Cap_RunAsAdmin.exe"), new StringBuilder(""), new StringBuilder(""), 1);

            Console.WriteLine("休眠{0}秒，等待游戏启动成功，{0}秒后会自动退出", sleepSecond);
            // 休眠10秒，再还原时间
            Thread.Sleep(sleepSecond * 1000);
            Console.WriteLine("还原时间，并退出");
            SetLocalTime(ref systemTime);

            //Console.ReadKey();
        }
    }
}
