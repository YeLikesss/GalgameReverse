using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Utils.Win32
{
    public class BaseAPI
    {
        /// <summary>
        /// 关闭句柄
        /// </summary>
        /// <param name="handle">句柄对象</param>
        /// <returns>True为关闭成功 False为关闭失败</returns>
        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr handle);
    }
}
