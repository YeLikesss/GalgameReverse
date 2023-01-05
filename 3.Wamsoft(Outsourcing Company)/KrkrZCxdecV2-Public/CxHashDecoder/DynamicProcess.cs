using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Utils.ArchiveInformation;
using Utils.Binary;
using Utils.Win32;

namespace CxHashDecoder
{
    internal class CommandLineArguments
    {
        /// <summary>
        /// 游戏进程ID
        /// </summary>
        public int ProcessID { get; private set; }
        /// <summary>
        /// 计算Hash函数指针
        /// </summary>
        public IntPtr HashCalculateFunctionPtr { get; private set; }

        private CommandLineArguments(int pid, IntPtr exportFunctionPtr)
        {
            ProcessID = pid;
            HashCalculateFunctionPtr = exportFunctionPtr;
        }

        /// <summary>
        /// 获取命令行参数
        /// </summary>
        /// <returns></returns>
        public static CommandLineArguments Get()
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                args = RuntimeHelpers.GetSubArray(args, Range.StartAt(1));

                //MessageBox.Show(args[0], "ProcessID");
                //MessageBox.Show(args[1], "ExportFunction");

                int pid = Convert.ToInt32(args[0], 16);
                IntPtr exportFuncPtr = new(Convert.ToInt32(args[1], 16));

                return new(pid, exportFuncPtr);
            }
            catch 
            { 
                return null;
            }
        }
    }

    /// <summary>
    /// 动态功能
    /// </summary>
    internal class DynamicProcess
    {
        /// <summary>
        /// 环境变量
        /// </summary>
        private class Context
        {
            /// <summary>
            /// 远程对象地址
            /// </summary>
            public IntPtr BaseAddress;
            /// <summary>
            /// 待计算字符串地址
            /// </summary>
            public IntPtr String;
            /// <summary>
            /// 待计算字符串长度
            /// </summary>
            public int StringLength;
            /// <summary>
            /// 返回值地址
            /// </summary>
            public IntPtr ReturnHash;
            /// <summary>
            /// 返回值长度
            /// </summary>
            public int ReturnLength;

            /// <summary>
            /// 计算模式
            /// </summary>
            public uint CalculateMode;
            /// <summary>
            /// 完成标志
            /// </summary>
            public uint CompleteFlag;

            /// <summary>
            /// 设置返回值长度
            /// </summary>
            /// <param name="mode">计算模式</param>
            public void SetReturnLength(uint mode)
            {
                if (mode == 1)
                {
                    this.ReturnLength = 8;
                }
                else if (mode == 2)
                {
                    this.ReturnLength = 32;
                }
                else
                {
                    this.ReturnLength = 0;
                }
            }

            /// <summary>
            /// 获取String字段地址
            /// </summary>
            /// <returns></returns>
            public IntPtr GetStringAddress()
            {
                return this.BaseAddress + 0;
            }
            /// <summary>
            /// 获取计算长度字段地址
            /// </summary>
            /// <returns></returns>
            public IntPtr GetStringLengthAddress()
            {
                return this.BaseAddress + 8;
            }
            /// <summary>
            /// 获取返回值字段地址
            /// </summary>
            /// <returns></returns>
            public IntPtr GetReturnHashAddress()
            {
                return this.BaseAddress + 16;
            }
            /// <summary>
            /// 获取返回值长度字段地址
            /// </summary>
            /// <returns></returns>
            public IntPtr GetReturnLengthAddress()
            {
                return this.BaseAddress + 24;
            }

            /// <summary>
            /// 获取计算模式字段地址
            /// </summary>
            /// <returns></returns>
            public IntPtr GetCalculateModeAddress()
            {
                return this.BaseAddress + 32;
            }

            /// <summary>
            /// 获取完成标志字段地址
            /// </summary>
            /// <returns></returns>
            public IntPtr GetCompleteFlagAddress()
            {
                return this.BaseAddress + 36;
            }
        }

        //执行代码
        private byte[] mShellCode = new byte[]
        {
            /*
            push ebp
            mov ebp,esp
            mov ecx,dword ptr ss:[ebp+8]    //取Context地址
            push dword ptr ds:[ecx+20]      //Context->CalculateMode
            push dword ptr ds:[ecx+8]       //Context->StringLength
            push dword ptr ds:[ecx]         //Context->String
            push dword ptr ds:[ecx+18]      //Context->ReturnLength
            push dword ptr ds:[ecx+10]      //Context->ReturnHash
            mov eax,0       //此处eax改为目标地址
            call eax
            xor ecx,ecx
            test eax,eax
            setne cl
            mov eax,dword ptr ss:[ebp+8]    
            mov dword ptr ds:[eax+24],ecx   //Context->CompleteFlag = (bool)ReturnValue
            mov esp,ebp
            pop ebp
            ret 4
            */

            0x55, 
            0x8B, 0xEC, 
            0x8B, 0x4D, 0x08, 
            0xFF, 0x71, 0x20, 
            0xFF, 0x71, 0x08, 
            0xFF, 0x31, 
            0xFF, 0x71, 0x18, 
            0xFF, 0x71, 0x10, 
            0xB8, 0x00, 0x00, 0x00, 0x00, 
            0xFF, 0xD0, 
            0x33, 0xC9, 
            0x85, 0xC0, 
            0x0F, 0x95, 0xC1, 
            0x8B, 0x45, 0x08, 
            0x89, 0x48, 0x24, 
            0x8B, 0xE5, 
            0x5D, 
            0xC2, 0x04, 0x00
        };
        //调用地址偏移
        private const int mShellCodeCallAddressOffset = 0x15;

        private CommandLineArguments commandLine;       //命令行
        private Process mParentProcess;     //游戏进程

        private IntPtr mExecutePoolMemory = IntPtr.Zero;        //执行池内存地址
        private IntPtr mContextPoolMemory = IntPtr.Zero;        //环境变量内存地址

        private List<Context> mContext = new(8);        //环境变量结构

        /// <summary>
        /// 获取进程句柄
        /// </summary>
        /// <returns></returns>
        private IntPtr GetProcessHandle()
        {
            return this.mParentProcess.Handle;
        }

        /// <summary>
        /// 获取函数地址
        /// </summary>
        /// <returns></returns>
        private IntPtr GetFunctionAddress()
        {
            return this.commandLine.HashCalculateFunctionPtr;
        }


        /// <summary>
        /// 初始化游戏进程
        /// </summary>
        private void Initialize()
        {
            if (this.commandLine != null)
            {
                try
                {
                    Process process = Process.GetProcessById(this.commandLine.ProcessID);
                    this.mParentProcess = process;
                }
                catch { }
            }
        }

        /// <summary>
        /// 生成远程环境变量
        /// </summary>
        /// <returns></returns>
        private void GenerateRemoteContext()
        {
            if (this.IsDynamicMode())
            {
                IntPtr contextMem = MemoryAPI.VirtualAllocEx(this.GetProcessHandle(), IntPtr.Zero, new(0x1000), MemoryAPI.MemoryAllocationType.Commit | MemoryAPI.MemoryAllocationType.Reserve, MemoryAPI.MemoryProtect.ReadWrite);
                this.mContextPoolMemory = contextMem;
                //生成8个环境变量  8线程
                for(int i = 0; i < 8; ++i)
                {
                    IntPtr strBuffer= MemoryAPI.VirtualAllocEx(this.GetProcessHandle(), IntPtr.Zero, new(0x1000), MemoryAPI.MemoryAllocationType.Commit | MemoryAPI.MemoryAllocationType.Reserve, MemoryAPI.MemoryProtect.ReadWrite);
                    Context context = new()
                    {
                        BaseAddress = contextMem,
                        String = strBuffer,
                        StringLength = 0,
                        ReturnHash = contextMem + 0x100,
                        ReturnLength = 0,
                        CalculateMode = 0,
                        CompleteFlag = 0
                    };
                    contextMem += 0x200;
                    this.mContext.Add(context);
                }

                //修改shellcode
                {
                    Span<byte> shellCode = this.mShellCode.AsSpan();
                    BitConverter.TryWriteBytes(shellCode.Slice(DynamicProcess.mShellCodeCallAddressOffset, 4), this.GetFunctionAddress().ToInt32());
                }

                //申请内存并写入shellcode
                this.mExecutePoolMemory = MemoryAPI.VirtualAllocEx(this.GetProcessHandle(), IntPtr.Zero, new(0x1000), MemoryAPI.MemoryAllocationType.Commit | MemoryAPI.MemoryAllocationType.Reserve, MemoryAPI.MemoryProtect.ExecuteReadWrite);
                {
                    MemoryAPI.WriteProcessMemory(this.GetProcessHandle(), this.mExecutePoolMemory, this.mShellCode, new(this.mShellCode.Length), out IntPtr writen);
                }
            }

        }


        private static DynamicProcess smInstance = null;
        /// <summary>
        /// 对象实例
        /// </summary>
        public static DynamicProcess Instance
        {
            get
            {
                smInstance ??= new();
                return smInstance;
            }
        }


        /// <summary>
        /// 获取是否为动态模式  (仅支持32位)
        /// </summary>
        /// <returns></returns>
        public bool IsDynamicMode()
        {
            return this.commandLine != null && this.mParentProcess != null && IntPtr.Size == 4;
        }

        /// <summary>
        /// 进程是否已退出
        /// </summary>
        /// <returns></returns>
        public bool IsExit()
        {
            return this.mParentProcess.HasExited;
        }

        /// <summary>
        /// 远程计算文本
        /// </summary>
        /// <param name="texts">文本数组</param>
        /// <param name="mode">1->文件夹路径计算 2->文件名路径计算</param>
        /// <returns>字典对象 Key->Hash值(文本型) Value->原文本</returns>
        public IEnumerable<Dictionary<string, string>> RemoteHashCalculate(List<string> texts, uint mode)
        {
            Dictionary<string, string> result = new(8);

            int count = texts.Count;
            int loopCount = (count / 8) + 1;
            int pos = 0;

            for (int i = 0; i < loopCount; ++i)
            {
                int procCount = Math.Min(count - pos, 8);
                List<string> group = texts.GetRange(pos, procCount);
                pos += procCount;

                for (int t = 0; t < procCount; t++) 
                {
                    Context context = this.mContext[t];
                    context.CalculateMode = mode;
                    context.CompleteFlag = 0;

                    string s = group[t];
                    context.StringLength = s.Length;
                    context.SetReturnLength(mode);

                    byte[] buffer = Encoding.Unicode.GetBytes(s);
                    //写入文本
                    MemoryAPI.WriteProcessMemory(this.GetProcessHandle(), context.String, buffer, new(buffer.Length), out IntPtr writen);

                    //写入环境变量
                    {
                        MemoryAPI.WriteProcessMemory(this.GetProcessHandle(), context.GetStringAddress(), BitConverter.GetBytes(context.String.ToInt32()), new(4), out IntPtr w1);
                        MemoryAPI.WriteProcessMemory(this.GetProcessHandle(), context.GetStringLengthAddress(), BitConverter.GetBytes(context.StringLength), new(4), out IntPtr w2);
                        MemoryAPI.WriteProcessMemory(this.GetProcessHandle(), context.GetReturnHashAddress(), BitConverter.GetBytes(context.ReturnHash.ToInt32()), new(4), out IntPtr w3);
                        MemoryAPI.WriteProcessMemory(this.GetProcessHandle(), context.GetReturnLengthAddress(), BitConverter.GetBytes(context.ReturnLength), new(4), out IntPtr w4);
                        MemoryAPI.WriteProcessMemory(this.GetProcessHandle(), context.GetCalculateModeAddress(), BitConverter.GetBytes(context.CalculateMode), new(4), out IntPtr w5);
                        MemoryAPI.WriteProcessMemory(this.GetProcessHandle(), context.GetCompleteFlagAddress(), BitConverter.GetBytes(context.CompleteFlag), new(4), out IntPtr w6);
                    }
                    IntPtr handle = ThreadAPI.CreateRemoteThread(this.GetProcessHandle(), IntPtr.Zero, IntPtr.Zero, this.mExecutePoolMemory, context.BaseAddress, ThreadAPI.ThreadCreateFlag.Immediately, out uint id);
                    BaseAPI.CloseHandle(handle);
                }

                //等待计算
                {
                    byte[] retFlag = new byte[4];
                    uint flag;
                    do
                    {
                        flag = 0;
                        for(int t = 0; t < procCount; ++t)
                        {
                            Context context = this.mContext[t];
                            //读取标志位
                            MemoryAPI.ReadProcessMemory(this.GetProcessHandle(), context.GetCompleteFlagAddress(), retFlag, new(4), out IntPtr read);
                            flag += BitConverter.ToUInt32(retFlag);
                        }
                    }
                    while (flag != procCount);
                }

                //计算完毕
                {
                    result.Clear();
                    for (int t = 0; t < procCount; ++t)
                    {
                        Context context = this.mContext[t];
                        byte[] hash = new byte[context.ReturnLength];
                        //读取hash
                        MemoryAPI.ReadProcessMemory(this.GetProcessHandle(), context.ReturnHash, hash, new(hash.Length), out IntPtr read);
                        result.TryAdd(BinaryDataConvert.HexToString(hash), group[t]);
                    }
                    yield return result;
                }

            }
        }



        public DynamicProcess()
        {
            this.commandLine = CommandLineArguments.Get();
            this.Initialize();
            this.GenerateRemoteContext();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this.IsDynamicMode())
            {
                MemoryAPI.VirtualFreeEx(this.GetProcessHandle(), this.mContextPoolMemory, IntPtr.Zero, MemoryAPI.MemoryFreeType.Release);
                MemoryAPI.VirtualFreeEx(this.GetProcessHandle(), this.mExecutePoolMemory, IntPtr.Zero, MemoryAPI.MemoryFreeType.Release);

                foreach (var context in this.mContext)
                {
                    MemoryAPI.VirtualFreeEx(this.GetProcessHandle(), context.String, IntPtr.Zero, MemoryAPI.MemoryFreeType.Release);
                }
                this.commandLine = null;
                this.mParentProcess.Dispose();
                this.mParentProcess = null;
            }
        }

        ~DynamicProcess()
        {
            this.Dispose();
        }
    }
}
