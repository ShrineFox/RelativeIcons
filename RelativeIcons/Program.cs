using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Text.RegularExpressions;
using System.Reflection;

namespace RelativeIcons
{
    class Program
    {
        static List<string> folders = new List<string>();

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        class Win32
        {
            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        }

        // https://stackoverflow.com/questions/21113058/how-to-set-relative-path-for-icons-in-windows
        static void Main(string[] args)
        {
            string path = args[0];
            if (!Directory.Exists(path))
                return;

            foreach (var dir in GetAllSubfoldersRecursively(path.Substring(0,1) + ":\\", 3))
            {
                if (GetIconPath(dir).ToLower().Contains(".ico"))
                {
                    Console.WriteLine(dir);
                    Console.WriteLine($"Icon: {GetIconPath(dir).Remove(0, 2)}\n");
                    string driveletter = path.Substring(0, 1);
                    string newpath = dir.Replace($"{driveletter}:\\", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\");
                    string relativePath = "";
                    for (int i = 0; i < dir.Split('\\').Length - 1; i++)
                    {
                        relativePath += "..\\";
                    }
                    Directory.CreateDirectory(newpath);
                    File.WriteAllText($"{Path.Combine(newpath, "desktop.ini")}", $"[.ShellClassInfo]\nIconResource=\"{GetIconPath(dir).Replace(path,relativePath)}\",0");
                }
            }
            Console.WriteLine("\nDone");
            Console.ReadKey();
        }

        // https://stackoverflow.com/questions/26205607/getting-folder-icon-path
        public static string GetIconPath(string folderPath)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            Win32.SHGetFileInfo(folderPath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), (int)0x1000);
            return shinfo.szDisplayName;
        }

        // https://stackoverflow.com/questions/37441459/unauthorizedaccessexception-with-getdirectories
        private static List<string> GetAllSubfoldersRecursively(string path, int depth)
        {
            try
            {
                foreach (string folder in Directory.GetDirectories(path).Where(x => x.Split('\\').Length <= depth))
                {
                    folders.Add(folder);
                    GetAllSubfoldersRecursively(folder, depth);
                }
            }
            catch (UnauthorizedAccessException) { }

            return folders;
        }

    }
}
