using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Utils.Win32
{
    public class MemoryAPI
    {
        /// <summary>
        /// 内存申请类型
        /// </summary>
        [Flags]
        public enum MemoryAllocationType : uint
        {
            /// <summary>
            /// 提交
            /// </summary>
            /// <remarks>
            /// 为指定的保留内存页面分配内存 该函数还保证当调用者稍后最初访问内存时，内容将为零。除非实际访问虚拟地址，否则不会分配实际的物理页面。
            /// </remarks>
            Commit = 0x00001000,
            /// <summary>
            /// 保留
            /// </summary>
            /// <remarks>
            /// 保留进程的虚拟地址空间范围，而不在内存或磁盘上的页面文件中分配任何实际物理存储。
            /// </remarks>
            Reserve = 0x00002000,
            /// <summary>
            /// 重置
            /// </summary>
            /// <remarks>
            /// 不从页面文件读取或写入页面
            /// 内存块稍后会再次使用，不取消提交。此值不能与任何其他值一起使用。
            /// 使用此值并不能保证使用Reset操作的范围将包含零。如果您希望范围包含零，请取消提交内存，然后重新提交。
            /// </remarks>
            Reset = 0x00080000,
            /// <summary>
            /// 取消重置
            /// </summary>
            /// <remarks>
            /// 只能在之前成功应用Reset的地址范围上调用
            /// 尝试反转Reset的效果
            /// 如果函数成功，则意味着指定地址范围内的所有数据都完好无损。
            /// 如果函数失败，则地址范围内的至少一些数据已被零替换。
            /// </remarks>
            ResetUndo = 0x01000000,
            /// <summary>
            /// 大页面支持
            /// </summary>
            /// <remarks>
            /// 需指定Reserve和Commit
            /// 要获取此值 请使用GetLargePageMinimum函数
            /// </remarks>
            LargePages = 0x20000000,
            /// <summary>
            /// 物理地址扩展
            /// </summary>
            /// <remarks>
            /// 必须与Reserve一起使用
            /// </remarks>
            Physical = 0x00400000,
            /// <summary>
            /// 在高地址分配内存
            /// </summary>
            TopDown = 0x00100000,
            /// <summary>
            /// 指定跟踪写入的页面
            /// </summary>
            /// <remarks>
            /// 必须指定Reverse 
            /// 调用GetWriteWatch函数
            /// 重置跟踪状态请调用GetWriteWatch或 ResetWriteWatch
            /// 内存区域的写入跟踪功能保持启用，直到该区域被释放。
            /// </remarks>
            WriteWatch = 0x00200000
        }
        /// <summary>
        /// 内存保护属性
        /// </summary>
        [Flags]
        public enum MemoryProtect : uint
        {
            /// <summary>
            /// 可执行
            /// </summary>
            /// <remarks>CreateFileMapping不支持此标志</remarks>
            Execute = 0x00000010,
            /// <summary>
            /// 可读可执行
            /// </summary>
            /// <remarks></remarks>
            ExecuteRead = 0x00000020,
            /// <summary>
            /// 可读可写可执行
            /// </summary>
            /// <remarks></remarks>
            ExecuteReadWrite = 0x00000040,
            /// <summary>
            /// 可执行 只读或写复制
            /// </summary>
            /// <remarks>VirtualAlloc不支持此标志</remarks>
            ExecuteWriteCopy = 0x00000080,
            /// <summary>
            /// 禁止访问
            /// </summary>
            /// <remarks>CreateFileMapping不支持此标志</remarks>
            NoAccess = 0x00000001,
            /// <summary>
            /// 只读
            /// </summary>
            /// <remarks></remarks>
            ReadOnly = 0x00000002,
            /// <summary>
            /// 可读可写
            /// </summary>
            /// <remarks></remarks>
            ReadWrite = 0x00000004,
            /// <summary>
            /// 只读或写复制
            /// </summary>
            /// <remarks>VirtualAlloc不支持此标志</remarks>
            WriteCopy = 0x00000008,
            /// <summary>
            /// 设置为CFG无效目标
            /// </summary>
            /// <remarks>VirtualAlloc CreateFileMapping不支持此标志</remarks>
            TargetsInvalid = 0x40000000,
            /// <summary>
            /// VirtualProtect保护修改发生变化  不更新CFG信息
            /// </summary>
            /// <remarks></remarks>
            TargetsNoUpdate = 0x40000000,
            /// <summary>
            /// 设置为保护页面(一次性)
            /// </summary>
            /// <remarks>
            /// CreateFileMapping不支持此标志
            /// 不能与NoAccess一起使用
            /// 任何访问保护页的尝试都会导致系统引发STATUS GUARD PAGE VIOLATION 异常并关闭保护页状态
            /// </remarks>
            Gurad = 0x00000100,
            /// <summary>
            /// 不可缓存
            /// </summary>
            /// <remarks>
            /// 只能VirtualAlloc分配私有内存使用
            /// 不能与Guard NoAccess WriteCombine
            /// 将互锁函数与使用SecNoCache映射的内存一起使用可能会导致EXCEPTION ILLEGAL INSTRUCTION异常 
            /// </remarks>
            NoCache = 0x00000200,
            /// <summary>
            /// 设置为写入组合
            /// </summary>
            /// <remarks>
            /// 将互锁函数与映射为写入组合的内存一起使用可能会导致EXCEPTION ILLEGAL INSTRUCTION异常
            /// 不能与Guard NoAccess NoCache使用
            /// </remarks>
            WriteCombine = 0x00000400
        }

        public enum MemoryFreeType : uint
        {
            /// <summary>
            /// 取消提交页面的指定区域
            /// </summary>
            /// <remarks>操作后 页面处于保留状态</remarks>
            Decommit = 0x00004000,
            /// <summary>
            /// 释放
            /// </summary>
            /// <remarks>size必须为0</remarks>
            Release = 0x00008000
        }

        /// <summary>
        /// 分配进程内存
        /// </summary>
        /// <param name="hProcess">进程句柄</param>
        /// <param name="address">内存地址</param>
        /// <param name="size">内存大小</param>
        /// <param name="type">分配类型</param>
        /// <param name="protect">内存属性</param>
        /// <returns>内存地址</returns>
        [DllImport("kernel32.dll", EntryPoint = "VirtualAllocEx", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr address, IntPtr size, MemoryAllocationType type, MemoryProtect protect);

        /// <summary>
        /// 释放进程内存
        /// </summary>
        /// <param name="hProcess">进程句柄</param>
        /// <param name="address">内存地址</param>
        /// <param name="size">内存大小</param>
        /// <param name="type">释放类型</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "VirtualFreeEx", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr address, IntPtr size, MemoryFreeType type);

        /// <summary>
        /// 进程读内存
        /// </summary>
        /// <param name="hProcess">进程句柄</param>
        /// <param name="address">内存大小</param>
        /// <param name="buffer">待读取数据缓存</param>
        /// <param name="size">读取数量</param>
        /// <param name="read">已读取数量</param>
        /// <returns>True 写入成功 False写入失败</returns>
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr address, IntPtr buffer, IntPtr size, out IntPtr read);

        /// <summary>
        /// 进程读内存
        /// </summary>
        /// <param name="hProcess">进程句柄</param>
        /// <param name="address">内存大小</param>
        /// <param name="buffer">待读取数据缓存</param>
        /// <param name="size">读取数量</param>
        /// <param name="read">已读取数量</param>
        /// <returns>True 写入成功 False写入失败</returns>
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr address, byte[] buffer, IntPtr size, out IntPtr read);

        /// <summary>
        /// 进程写内存
        /// </summary>
        /// <param name="hProcess">进程句柄</param>
        /// <param name="address">内存大小</param>
        /// <param name="buffer">待写入数据缓存</param>
        /// <param name="size">写入数量</param>
        /// <param name="writen">已写入数量</param>
        /// <returns>True 写入成功 False写入失败</returns>
        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr address, byte[] buffer, IntPtr size, out IntPtr writen);

        /// <summary>
        /// 进程写内存
        /// </summary>
        /// <param name="hProcess">进程句柄</param>
        /// <param name="address">内存大小</param>
        /// <param name="buffer">待写入数据缓存</param>
        /// <param name="size">写入数量</param>
        /// <param name="writen">已写入数量</param>
        /// <returns>True 写入成功 False写入失败</returns>
        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr address, IntPtr buffer, IntPtr size, out IntPtr writen);
    }
}
