using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Win32
{
    public class ThreadAPI
    {
        /// <summary>
        /// 线程参数
        /// </summary>
        public enum ThreadCreateFlag : uint
        {
            /// <summary>
            /// 立刻运行
            /// </summary>
            Immediately = 0x00000000,
            /// <summary>
            /// 挂起线程
            /// </summary>
            Suspened = 0x00000004,
            /// <summary>
            /// 指定栈保留大小，未指定则为提交大小
            /// </summary>
            StackSizeParamIsAReservation = 0x00010000
        }

        /// <summary>
        /// 创建远程线程
        /// </summary>
        /// <param name="hProcess">进程句柄</param>
        /// <param name="lpSecurityAttributes">安全属性</param>
        /// <param name="stackSize">栈初始大小</param>
        /// <param name="startAddress">启动运行地址</param>
        /// <param name="parameter">启动函数参数</param>
        /// <param name="creationFlag">线程属性</param>
        /// <param name="threadID">线程ID</param>
        /// <returns>线程句柄</returns>
        [DllImport("kernel32.dll", EntryPoint = "CreateRemoteThread", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpSecurityAttributes, IntPtr stackSize, IntPtr startAddress, IntPtr parameter, ThreadCreateFlag creationFlag, out uint threadID);

        /// <summary>
        /// 挂起线程
        /// </summary>
        /// <param name="hThread">线程句柄</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "SuspendThread", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public static extern uint SuspendThread(IntPtr hThread);

        /// <summary>
        /// 恢复线程
        /// </summary>
        /// <param name="hThread">线程句柄</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "ResumeThread", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public static extern uint ResumeThread(IntPtr hThread);
    }
}
