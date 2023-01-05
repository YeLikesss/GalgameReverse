#include "Base.h"
#include "pe.h"
#include "path.h"
#include "file.h"
#include "util.h"
#include "stringhelper.h"
#include "encoding.h"
#include "log.h"
#include "KrPath.h"
#include "detours.h"
#include "cJSON.h"
#include "IndexProc.h"
#include "ArchiveProc.h"

#pragma warning ( push )
#pragma warning ( disable : 4100 4201 4457 )
#include "tp_stub.h"
#pragma warning ( pop )

template<class T>
void InlineHook(T& OriginalFunction, T DetourFunction)
{
    DetourUpdateThread(GetCurrentThread());
    DetourTransactionBegin();
    DetourAttach(&(PVOID&)OriginalFunction, (PVOID&)DetourFunction);
    DetourTransactionCommit();
}

template<class T>
void UnInlineHook(T& OriginalFunction, T DetourFunction)
{
    DetourUpdateThread(GetCurrentThread());
    DetourTransactionBegin();
    DetourDetach(&(PVOID&)OriginalFunction, (PVOID&)DetourFunction);
    DetourTransactionCommit();
}

//*******************配置相关*******************//
#define PathHasherMode 1        //文件夹hash计算模式
#define FileHasherMode 2        //文件名hash计算模式


static std::wstring g_dllPath;          //本dll路径

static bool g_enableExtract = false;    //解包启用标志
static bool g_enableHasher = false;     //hash启用标志
static bool g_enableDynamicHashDecoder = false;     //Hash计算器启用标志

static DWORD g_extractDelayTimes = 15000;       //解包延时时间

static wchar_t g_tableSplit[] = L"##YSig##";    //文件表分割符
static wchar_t g_tableNewLine[] = L"\n";        //文件表换行符 [LF]
static wchar_t g_EmptyStringReplace[] = L"%EmptyString%";     //空文件夹替代


static std::wstring g_currentDirPath;	    //当前游戏文件夹路径
static std::wstring g_extractDirPath;       //资源解包输出文件夹路径
static std::wstring g_hashExtractDirPath;   //hash输出路径

//*********************基本函数************************//

//获取TJS字符串
const tjs_char* TJSStringGetPtr(tTJSString* s)
{
    if (!s)
        return L"";

    tTJSVariantString_S* v = *(tTJSVariantString_S**)s;

    if (!v)
        return L"";

    if (v->LongString)
        return v->LongString;

    return v->ShortString;
}

//*******************插件初始化相关*******************//

extern "C"
{
    typedef HRESULT(_stdcall* tTVPV2LinkProc)(iTVPFunctionExporter*);
    typedef HRESULT(_stdcall* tTVPV2UnlinkProc)();
}

static bool g_IsTVPStubInitialized = false;     //插件初始化标志

extern "C" __declspec(dllexport) HRESULT _stdcall V2Link(iTVPFunctionExporter* exporter) 
{ 
    g_IsTVPStubInitialized = TVPInitImportStub(exporter);
    return S_OK; 
}
extern "C" __declspec(dllexport) HRESULT _stdcall V2Unlink() { return S_OK; }

//插件功能Hook
tTVPV2LinkProc pfnV2Link = NULL;		//原插件入口

HRESULT _stdcall HookV2Link(iTVPFunctionExporter* exporter)
{
    HRESULT result = pfnV2Link(exporter);
    V2Link(exporter);   //插件初始化

    UnInlineHook(pfnV2Link, HookV2Link);

    TVPSetCommandLine(L"-debugwin", L"yes");
    return result;
}


//*******************资源提取相关***********************//

static std::vector<std::wstring> g_packageFiles;    //待解包数组


#define CX_CREATESTREAM_SIGNATURE "\x55\x8B\xEC\x6A\xFF\x68\x2A\x2A\x2A\x2A\x64\xA1\x00\x00\x00\x00\x50\x51\xA1\x2A\x2A\x2A\x2A\x33\xC5\x50\x8D\x45\xF4\x64\xA3\x00\x00\x00\x00\xA1\x2A\x2A\x2A\x2A\x85\xC0\x75\x32\x68\xB0\x30\x00\x00"
#define CX_CREATESTREAM_SIGNATURE_LENGTH ( sizeof(CX_CREATESTREAM_SIGNATURE) -1 )

#define CX_PARSERARCHIVEINDEX_SIGNATURE "\x55\x8B\xEC\x6A\xFF\x68\x2A\x2A\x2A\x2A\x64\xA1\x00\x00\x00\x00\x50\x83\xEC\x14\x57\xA1\x2A\x2A\x2A\x2A\x33\xC5\x50\x8D\x45\xF4\x64\xA3\x00\x00\x00\x00\x83\x7D\x08\x00\x0F\x84\x2A\x2A\x00\x00\xA1\x2A\x2A\x2A\x2A\x85\xC0\x75\x12\x68\x2A\x2A\x2A\x2A\xE8\x2A\x2A\x2A\x2A\x83\xC4\x04\xA3\x2A\x2A\x2A\x2A\xFF\x75\x0C\x8D\x4D\xF0\x51\xFF\xD0\xA1\x2A\x2A\x2A\x2A\xC7\x45\xFC\x00\x00\x00\x00\x85\xC0"
#define CX_PARSERARCHIVEINDEX_SIGNATURE_LENGTH ( sizeof(CX_PARSERARCHIVEINDEX_SIGNATURE) -1 )

typedef tjs_int32(_cdecl* tCxParserArchiveIndex)(tTJSVariant* retValue, tTJSVariant* tjsXP3Name);
tCxParserArchiveIndex CxParserArchiveIndexPtr = NULL;   //Cx解析文件表函数指针


//此处CryptoFilterStream 继承 IStream
typedef IStream* (_cdecl* tCxCreateStream)(tTJSString* fakeName, tjs_int64 key, tjs_uint32 encryptMode);
tCxCreateStream CxCreateStreamPtr = NULL;       //CxCreateStream函数指针


IStream* CxCreateStreamWithIndex(ArchiveListBinary*, std::wstring&);
bool CxGetArchiveList(std::vector<ArchiveListBinary>&, std::wstring&);
void ExtractFile(IStream*, std::wstring&);
void ExtractorWithPackageName(std::wstring&);
void ArchiveExtractor();


//获取并解析文件表
bool CxGetArchiveList(std::vector<ArchiveListBinary>& retList, std::wstring& xp3PackageFilePath)
{
    retList.clear();

    //加载文件表
    tTJSVariant tjsList = tTJSVariant();
    tTJSVariant tjsXp3FilePath(xp3PackageFilePath.c_str());
    CxParserArchiveIndexPtr(&tjsList, &tjsXp3FilePath);

    //读取文件表数组
    if (tjsList.Type() == tvtObject) 
    {
        tTJSVariantClosure& list = tjsList.AsObjectClosureNoAddRef();
        iTJSDispatch2* listObj = list.Object;
        iTJSDispatch2* listObjThis = list.ObjThis == NULL ? list.Object : list.ObjThis;

        //读取数组项数
        tTJSVariant tjsCount = tTJSVariant();
        listObj->PropGet(TJS_MEMBERMUSTEXIST, L"count", NULL, &tjsCount, listObjThis);
        long long count = tjsCount.AsInteger();

        if (count > 0) 
        {
            //获取文件夹项数 (占KR数组2项)
            //文件夹路径Hash
            //子文件数组
            int dirListSize = 2;
            int dirCount = (int)count / dirListSize;

            //遍历文件夹
            for (int di = 0; di < dirCount; ++di) 
            {
                //获取文件夹路径Hash与子文件表
                tTJSVariant tjsDirHash = tTJSVariant();
                tTJSVariant tjsFileList = tTJSVariant();
                listObj->PropGetByNum(TJS_CII_GET, di * dirListSize + 0, &tjsDirHash, listObjThis);
                listObj->PropGetByNum(TJS_CII_GET, di * dirListSize + 1, &tjsFileList, listObjThis);

                //获取Hash
                tTJSVariantOctet_S* dirHash = (tTJSVariantOctet_S*)(tjsDirHash.AsOctetNoAddRef());      

                //获取子文件表数组
                tTJSVariantClosure& fileList = tjsFileList.AsObjectClosureNoAddRef();
                iTJSDispatch2* fileListObj = fileList.Object;
                iTJSDispatch2* fileListObjThis = fileList.ObjThis == NULL ? fileList.Object : fileList.ObjThis;

                //获取子文件数组项数
                tjsCount.Clear();
                fileListObj->PropGet(TJS_MEMBERMUSTEXIST, L"count", NULL, &tjsCount, fileListObjThis);
                count = tjsCount.AsInteger();

                //获取文件项数 (占KR数组2项)
                //文件名Hash
                //文件信息
                int fileListSize = 2;
                int fileCount = (int)count / dirListSize;

                //遍历子文件
                for (int fi = 0; fi < fileCount; ++fi) 
                {
                    //获取文件名Hash与文件信息
                    tTJSVariant tjsFileNameHash = tTJSVariant();
                    tTJSVariant tjsFileInfo = tTJSVariant();
                    fileListObj->PropGetByNum(TJS_CII_GET, fi * fileListSize + 0, &tjsFileNameHash, fileListObjThis);
                    fileListObj->PropGetByNum(TJS_CII_GET, fi * fileListSize + 1, &tjsFileInfo, fileListObjThis);

                    //获取Hash
                    tTJSVariantOctet_S* fileNameHash = (tTJSVariantOctet_S*)(tjsFileNameHash.AsOctetNoAddRef());

                    //获取文件信息
                    tTJSVariantClosure& fileInfo = tjsFileInfo.AsObjectClosureNoAddRef();
                    iTJSDispatch2* fileInfoObj = fileInfo.Object;
                    iTJSDispatch2* fileInfoObjThis = fileInfo.ObjThis == NULL ? fileInfo.Object : fileInfo.ObjThis;

                    //获取文件信息
                    long long ordinal = 0;
                    long long key = 0;

                    tTJSVariant tjsValue = tTJSVariant();
                    fileInfoObj->PropGetByNum(TJS_CII_GET, 0, &tjsValue, fileInfoObjThis);
                    ordinal = tjsValue.AsInteger();

                    tjsValue.Clear();
                    fileInfoObj->PropGetByNum(TJS_CII_GET, 1, &tjsValue, fileInfoObjThis);
                    key = tjsValue.AsInteger();

                    //解析后的文件表
                    ArchiveListBinary arcListBin{ 0 };
                    memcpy(arcListBin.DirectoryPathHash, dirHash->Data, dirHash->Length);
                    memcpy(arcListBin.FileNameHash, fileNameHash->Data, fileNameHash->Length);

                    arcListBin.Key = key;
                    GetFakeNameWithOrdinal(ordinal, (wchar_t*)&arcListBin.FakeName);
                    arcListBin.EncryptMode = GetEncryptModeWithOrdinal(ordinal);

                    retList.push_back(arcListBin);
                }
            }
            return true;
        }
    }
    return false;
}

//创建解密流
IStream* CxCreateStreamWithIndex(ArchiveListBinary* arcIndex, std::wstring& packageName)
{
    std::wstring xp3Path = GetXP3VFSCurrentDirectoryPath(g_currentDirPath);   //获取xp3格式文件夹路径
    xp3Path += L"\\";
    xp3Path += packageName;         //添加封包名
    xp3Path += L">";           //添加封包后缀(带封包分隔符)
    xp3Path += (wchar_t*)arcIndex->FakeName;        //添加资源名

    NormalizeXP3VFSPath(xp3Path);   //规范化路径

    tTJSString tjsXP3Path(xp3Path.c_str());

    return CxCreateStreamPtr(&tjsXP3Path, arcIndex->Key, arcIndex->EncryptMode);
}

//提取资源
void ExtractFile(IStream* stream, std::wstring& extractPath)
{
    unsigned long long size = StreamGetLength(stream);	//获取流长度
    if (size > 0)
    {
        //创建文件夹
        std::wstring outputDir = Path::GetDirectoryName(extractPath);
        if (!outputDir.empty())
        {
            FullCreateDirectoryW(outputDir.c_str());
        }

        std::vector<uint8_t> buffer;

        bool success = false;

        if (TryDecryptText(stream, buffer))  //尝试解密SimpleEncrypt
        {
            success = true;
        }
        else
        {
            buffer.resize(size);  //调整动态数组容器大小

            stream->Seek(LARGE_INTEGER{ 0 }, STREAM_SEEK_SET, NULL);

            //读取KR资源流
            if (StreamRead(stream, buffer.data(), size) == size)
            {
                success = true;
            }
        }

        if (success && !buffer.empty())
        {
            OutputDebugStringW((L"Extract ---> " + extractPath).c_str());

            if (File::WriteAllBytes(extractPath, buffer.data(), buffer.size()) == false)  //回写文件
            {
                OutputDebugStringW((L"WriteError ---> " + extractPath).c_str());
            }
        }
        else 
        {
            OutputDebugStringW((L"InVaild File ---> " + extractPath).c_str());
        }
        stream->Seek(LARGE_INTEGER{ 0 }, STREAM_SEEK_SET, NULL);
    }
    else
    {
        OutputDebugStringW((L"EmptyFile ---> " + extractPath).c_str());
    }
}

//使用封包名解包
void ExtractorWithPackageName(std::wstring& packageFileName)
{
    std::wstring xp3PackagePath = NormalizeXP3PackagePath(GetXP3VFSCurrentDirectoryPath(g_currentDirPath), packageFileName);    //转换为标准封包路径
    std::vector<ArchiveListBinary> arcListBins = std::vector<ArchiveListBinary>();

    if (CxGetArchiveList(arcListBins, xp3PackagePath) && !arcListBins.empty()) 
    {
        FullCreateDirectoryW(g_extractDirPath);     //创建资源提取导出文件夹

        std::wstring extractOutput = g_extractDirPath + L"\\";	//导出文件夹
        std::wstring packageName = Path::GetFileNameWithoutExtension(packageFileName);   //封包名
        std::wstring fileTableOutput = extractOutput + packageName + L".alst";      //文件表输出路径

        //创建写入文件表
        HANDLE tableHandle = CreateFileW(fileTableOutput.c_str(), GENERIC_READ | GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
        if (tableHandle != INVALID_HANDLE_VALUE) 
        {
            //写表准备
            SetFilePointer(tableHandle, 0, NULL, FILE_BEGIN);

            wchar_t* tableSplit = g_tableSplit;
            unsigned int tableSplitSize = sizeof(g_tableSplit) - sizeof(wchar_t);

            wchar_t* tableNewLine = g_tableNewLine;
            unsigned int tableNewLineSize = sizeof(g_tableNewLine) - sizeof(wchar_t);

            DWORD bytesWriten = 0;

            //遍历文件表
            for (ArchiveListBinary& arcIndexBin : arcListBins)
            {
                std::wstring dirHash = BytesToHexStringW(arcIndexBin.DirectoryPathHash, sizeof(arcIndexBin.DirectoryPathHash)); //文件夹Hash字符串形式
                std::wstring fileNameHash = BytesToHexStringW(arcIndexBin.FileNameHash, sizeof(arcIndexBin.FileNameHash)); //文件名Hash字符串形式
                std::wstring arcOutputPath = extractOutput + packageName + L"\\" + dirHash + L"\\" + fileNameHash;	//该资源导出路径


                //写表  dirHash[Sign]dirHash[Sign]fileHash[Sign]fileHash[NewLine]
                WriteFile(tableHandle, dirHash.c_str(), dirHash.size() * sizeof(wchar_t), &bytesWriten, NULL);
                WriteFile(tableHandle, tableSplit, tableSplitSize, &bytesWriten, NULL);
                WriteFile(tableHandle, dirHash.c_str(), dirHash.size() * sizeof(wchar_t), &bytesWriten, NULL);
                WriteFile(tableHandle, tableSplit, tableSplitSize, &bytesWriten, NULL);

                WriteFile(tableHandle, fileNameHash.c_str(), fileNameHash.size() * sizeof(wchar_t), &bytesWriten, NULL);
                WriteFile(tableHandle, tableSplit, tableSplitSize, &bytesWriten, NULL);
                WriteFile(tableHandle, fileNameHash.c_str(), fileNameHash.size() * sizeof(wchar_t), &bytesWriten, NULL);

                WriteFile(tableHandle, tableNewLine, tableNewLineSize, &bytesWriten, NULL);

                //创建流
                IStream* stream = CxCreateStreamWithIndex(&arcIndexBin, packageFileName);
                if (stream)
                {
                    ExtractFile(stream, arcOutputPath);
                    stream->Release();
                }
            }

            FlushFileBuffers(tableHandle);
            CloseHandle(tableHandle);

            OutputDebugStringW((L"Extract Completed ---> " + packageFileName).c_str());
        }
        else
        {
            OutputDebugStringW((L"Create Archive Table Failed ---> " + fileTableOutput).c_str());
        }
    }
    else
    {
        OutputDebugStringW((L"InVaild PackageFile ---> " + packageFileName).c_str());
    }
}

//资源解包
void ArchiveExtractor() 
{
    Sleep(g_extractDelayTimes);     //延时
    for (std::wstring& s : g_packageFiles)
    {
        ExtractorWithPackageName(s);
    }
}


//*******************Hash相关***********************//

//Hash计算接口
class ICxStringHasher
{
public:
    virtual ICxStringHasher* Release(bool isHeap) = 0;
    virtual int Calculate(tTJSVariant* hashValueRet, tTJSString* str, tTJSString* seed) = 0;
};

//Hash计算函数类型  fastcall模拟thiscall
typedef int(_fastcall* tHasherCalcute)(PVOID thisObj, DWORD unusedEdx, tTJSVariant* hashValueRet, tTJSString* str, tTJSString* seed);

//Hash计算虚表布局
struct ICxStringHasherVptrTable
{
    PVOID Release;
    PVOID Calculate;
};


//路径Hasher结构
struct CxPathStringHasher
{
    ICxStringHasherVptrTable* VptrTable;       //虚表
    BYTE* Salt;         //盐指针
    int SaltSize;  //大小
    BYTE SaltData[0x10];    //盐数据
};

//文件名Hasher结构
struct CxFileStringHasher
{
    ICxStringHasherVptrTable* VptrTable;       //虚表
    BYTE* Salt;         //盐指针
    int SaltSize;  //大小
    BYTE SaltData[0x20];    //盐数据
};

//Cx插件储存管理
struct CxTVPStorageMedia
{
    PVOID* VptrTable;
    int RefCount;
    DWORD field_8;     //未知字段 +0x08
    tTJSString PreFix;      //资源前缀
    tTJSString HasherSeed;   //Hash盐
    CRITICAL_SECTION CriticalSection;

    //此处为 ArchiveEntryLoaded   已加载资源表
    BYTE Reserve[0x20];

    /* 
    std::vector<tTJSString>  ???
        tTJSString* Start;
        tTJSString* End;
        tTJSString* MaxEnd;
    */
    std::vector<tTJSString> PackageFileNameLoaded;      //已加载封包

    CxPathStringHasher* PathNameHasher;     //路径Hash对象
    CxFileStringHasher* FileNameHasher;     //文件名Hash对象
};

//初始化封包管理函数指针类型
typedef HRESULT(_cdecl* tInitCompoundStorageMedia)(CxTVPStorageMedia** retTVPStorageMedia, tTJSVariant* tjsVarPrefix, int argCount, PVOID saltGroups);

#define CX_INITSTORAGEMEDIA_SIGNATURE "\x55\x8B\xEC\x6A\xFF\x68\x2A\x2A\x2A\x2A\x64\xA1\x00\x00\x00\x00\x50\x83\xEC\x08\x56\xA1\x2A\x2A\x2A\x2A\x33\xC5\x50\x8D\x45\xF4\x64\xA3\x00\x00\x00\x00\xA1\x2A\x2A\x2A\x2A\x85\xC0\x75\x12\x68\x2A\x2A\x2A\x2A\xE8\x2A\x2A\x2A\x2A\x83\xC4\x04\xA3\x2A\x2A\x2A\x2A\x8B\x75\x0C\x56\xFF\xD0\x83\xF8\x02\x74\x2A\xB8\x15\xFC\xFF\xFF\x8B\x4D\xF4\x64\x89\x0D\x00\x00\x00\x00\x59\x5E\x8B\xE5\x5D\xC3"
#define CX_INITSTORAGEMEDIA_SIGNATURE_LENGTH ( sizeof(CX_INITSTORAGEMEDIA_SIGNATURE) - 1 )


static bool g_HasherInitialized = false;        //Hash功能初始化
static CxTVPStorageMedia* g_TVPStorageMedia = NULL;            //Cx资源管理
static CxPathStringHasher g_PathStringHasher = { 0 };          //路径名hash对象
static CxFileStringHasher g_FileStringHasher = { 0 };          //文件名hash对象

static Log::Logger g_PathHashDumpLog;         //路径名HashDump
static Log::Logger g_FileHashDumpLog;         //文件名HashDump

static ICxStringHasherVptrTable g_HookPathHasherVptr = { 0 };       //Hook路径Hash对象虚表
static ICxStringHasherVptrTable g_HookFileHasherVptr = { 0 };       //Hook文件名Hash对象虚表

static tHasherCalcute PathHashCalcPtr = NULL;   //路径hash计算函数指针
static tHasherCalcute FileHashCalcPtr = NULL;   //文件名hash计算函数指针

static tInitCompoundStorageMedia oInitCompoundStorageMediaPtr = NULL;       //初始化封包管理函数指针

int _fastcall HookPathHasherCalcute(CxPathStringHasher*, DWORD, tTJSVariant*, tTJSString*, tTJSString*);
int _fastcall HookFileHasherCalcute(CxFileStringHasher*, DWORD, tTJSVariant*, tTJSString*, tTJSString*);

//Hook游戏路径Hash计算  fastcall模拟thiscall
int _fastcall HookPathHasherCalcute(CxPathStringHasher* thisObj, DWORD unusedEdx, tTJSVariant* hashValueRet, tTJSString* str, tTJSString* seed)
{
    int result = PathHashCalcPtr(thisObj, 0, hashValueRet, str, seed);
    const wchar_t* relativeDirPath = TJSStringGetPtr(str);
    //空文件夹替换
    if (*relativeDirPath == L'\0')
    {
        relativeDirPath = g_EmptyStringReplace;
    }

    //获取Hash值
    tTJSVariantOctet_S* hashValue = (tTJSVariantOctet_S*)(hashValueRet->AsOctetNoAddRef());
    //打印
    g_PathHashDumpLog.WriteUnicode(L"%s%s%s%s", relativeDirPath, g_tableSplit, BytesToHexStringW(hashValue->Data, hashValue->Length).c_str(), g_tableNewLine);

    return result;
}
//Hook游戏文件名Hash计算  fastcall模拟thiscall
int _fastcall HookFileHasherCalcute(CxFileStringHasher* thisObj, DWORD unusedEdx, tTJSVariant* hashValueRet, tTJSString* str, tTJSString* seed)
{
    int result = FileHashCalcPtr(thisObj, 0, hashValueRet, str, seed);
    const wchar_t* fileName = TJSStringGetPtr(str);

    //获取Hash值
    tTJSVariantOctet_S* hashValue = (tTJSVariantOctet_S*)(hashValueRet->AsOctetNoAddRef());
    //打印
    g_FileHashDumpLog.WriteUnicode(L"%s%s%s%s",fileName, g_tableSplit, BytesToHexStringW(hashValue->Data, hashValue->Length).c_str(), g_tableNewLine);

    return result;
}

/// <summary>
/// 文本Hash计算器
/// </summary>
extern "C" __declspec(dllexport)BOOL WINAPI SStringHashCalcutor(byte * retValue, unsigned int retValueLength, wchar_t* text, int textLength, unsigned int mode)
{
    //初始化检查
    if (!g_HasherInitialized) 
    {
        return FALSE;
    }

    ICxStringHasher* hasher = NULL;
    //选择模式
    if (mode == PathHasherMode)
    {
        //路径Hash模式
        hasher = (ICxStringHasher*)&g_PathStringHasher;
    }
    else if (mode == FileHasherMode)
    {
        //文件名Hash模式
        hasher = (ICxStringHasher*)&g_FileStringHasher;
    }
    else
    {
        return FALSE;
    }

    tTJSVariant tjsHashValueRet = tTJSVariant();
    tjsHashValueRet.Clear();
    tTJSString tjsText = tTJSString(text, textLength);
    //计算
    hasher->Calculate(&tjsHashValueRet, &tjsText, &(g_TVPStorageMedia->HasherSeed));
    //获取Hash值
    tTJSVariantOctet_S* hashValue = (tTJSVariantOctet_S*)(tjsHashValueRet.AsOctetNoAddRef());

    //hash长度
    unsigned int retLength = hashValue->Length;     
    //长度判断
    if (retLength > retValueLength)
    {
        return FALSE;
    }
    else
    {
        memcpy(retValue, hashValue->Data, retLength);
    }
    return TRUE;
}


//Hook封包管理初始化函数
HRESULT _cdecl HookInitCompoundStorageMedia(CxTVPStorageMedia** retTVPStorageMedia, tTJSVariant* tjsVarPrefix, int argCount, PVOID saltGroups)
{
    HRESULT result = oInitCompoundStorageMediaPtr(retTVPStorageMedia, tjsVarPrefix, argCount, saltGroups);
    if (result == S_OK) 
    {
        CxTVPStorageMedia* storageMedia = *retTVPStorageMedia;

        //打印输出Hash盐值
        OutputDebugStringW((std::wstring(L"Hash Seed ---> ") + TJSStringGetPtr(&storageMedia->HasherSeed)).c_str());
        OutputDebugStringW((L"PathHasherSalt ---> " + BytesToHexStringW(storageMedia->PathNameHasher->Salt, storageMedia->PathNameHasher->SaltSize)).c_str());
        OutputDebugStringW((L"FileHasherSalt ---> " + BytesToHexStringW(storageMedia->FileNameHasher->Salt, storageMedia->FileNameHasher->SaltSize)).c_str());

        memcpy(&g_PathStringHasher, storageMedia->PathNameHasher, sizeof(CxPathStringHasher));      //保存路径Hash对象
        memcpy(&g_FileStringHasher, storageMedia->FileNameHasher, sizeof(CxFileStringHasher));      //保存文件名Hash对象

        //拷贝虚表
        memcpy(&g_HookPathHasherVptr, storageMedia->PathNameHasher->VptrTable, sizeof(ICxStringHasherVptrTable));   
        memcpy(&g_HookFileHasherVptr, storageMedia->FileNameHasher->VptrTable, sizeof(ICxStringHasherVptrTable));
        //修改虚表
        g_HookPathHasherVptr.Calculate = HookPathHasherCalcute;
        g_HookFileHasherVptr.Calculate = HookFileHasherCalcute;

        //保存原函数指针
        PathHashCalcPtr = (tHasherCalcute)storageMedia->PathNameHasher->VptrTable->Calculate;
        FileHashCalcPtr = (tHasherCalcute)storageMedia->FileNameHasher->VptrTable->Calculate;

        //Hook虚表
        storageMedia->PathNameHasher->VptrTable = &g_HookPathHasherVptr;
        storageMedia->FileNameHasher->VptrTable = &g_HookFileHasherVptr;

        g_TVPStorageMedia = storageMedia;       //保存封包管理对象
        g_HasherInitialized = true;         //初始化完成

        UnInlineHook(oInitCompoundStorageMediaPtr, HookInitCompoundStorageMedia);

        //启用Hash解码器
        if (g_enableDynamicHashDecoder)
        {
            std::wstring decoderPath = Path::GetDirectoryName(g_dllPath) + L"\\CxHashDecoder.exe";
            if (CheckFileExist(decoderPath)) 
            {
                wchar_t cmdArgs[128] = { 0 };

                DWORD pid = GetProcessId((HANDLE)-1);
                PVOID exportFunc = SStringHashCalcutor;
                
                std::wstring cmd = StringHelper::Format(L"/IPC %X %X", pid, exportFunc);

                memcpy(cmdArgs, cmd.c_str(), cmd.length() * 2);

                STARTUPINFOW si = { 0 };
                si.cb = sizeof(STARTUPINFOW);
                PROCESS_INFORMATION pi = { 0 };

                if (CreateProcessW(decoderPath.c_str(), cmdArgs, NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi))
                {
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);
                }
                else
                {
                    MessageBoxW(NULL, L"创建CxHashDecoder.exe进程失败", NULL, MB_OK);
                }
            }
            else
            {
                MessageBoxW(NULL, L"CxHashDecoder.exe不存在", NULL, MB_OK);
            }
        }
    }
    return result;
}


//启动提取程序
void WINAPI ExtractorProcess() 
{
    if (g_enableExtract)
    {
        CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)ArchiveExtractor, NULL, 0, NULL);
    }
    if (g_enableHasher)
    {
        InlineHook(oInitCompoundStorageMediaPtr, HookInitCompoundStorageMedia);
    }
}

// Original
auto pfnGetProcAddress = GetProcAddress;
// Hooked
FARPROC WINAPI HookGetProcAddress(HMODULE hModule, LPCSTR lpProcName)
{
    FARPROC result = pfnGetProcAddress(hModule, lpProcName);
    if (result)
    {
        // 忽略序号导出
        if (HIWORD(lpProcName) != 0)
        {
            if (strcmp(lpProcName, "V2Link") == 0)
            {
                //Nt头偏移
                PIMAGE_NT_HEADERS ntHeader = PIMAGE_NT_HEADERS((DWORD)hModule + ((PIMAGE_DOS_HEADER)hModule)->e_lfanew);
                //可选头大小
                DWORD optionalHeaderSize = ntHeader->FileHeader.SizeOfOptionalHeader;
                //第一个节表(代码段)
                PIMAGE_SECTION_HEADER codeSectionHeader = (PIMAGE_SECTION_HEADER)((DWORD)ntHeader + sizeof(ntHeader->Signature) + sizeof(IMAGE_FILE_HEADER) + optionalHeaderSize);

                DWORD codeStartRva = codeSectionHeader->VirtualAddress;  //代码段起始RVA
                DWORD codeSize = codeSectionHeader->SizeOfRawData;		//代码段大小

                DWORD codeStartVa = (DWORD)hModule + codeStartRva;      //代码段起始地址

                //Hook插件入口
                if (!g_IsTVPStubInitialized)
                {
                    pfnV2Link = (tTVPV2LinkProc)result;
                    InlineHook(pfnV2Link, HookV2Link);
                }

                //CxCreateStream接口
                if (!CxCreateStreamPtr)
                {
                    CxCreateStreamPtr = (tCxCreateStream)PE::SearchPattern((PVOID)codeStartVa, codeSize, CX_CREATESTREAM_SIGNATURE, CX_CREATESTREAM_SIGNATURE_LENGTH);
                }
                //CxParserArchiveIndex接口
                if(!CxParserArchiveIndexPtr)
                {
                    CxParserArchiveIndexPtr = (tCxParserArchiveIndex)PE::SearchPattern((PVOID)codeStartVa, codeSize, CX_PARSERARCHIVEINDEX_SIGNATURE, CX_PARSERARCHIVEINDEX_SIGNATURE_LENGTH);
                }

                //CompoundStorageMedia初始化接口
                if (!oInitCompoundStorageMediaPtr) 
                {
                    oInitCompoundStorageMediaPtr = (tInitCompoundStorageMedia)PE::SearchPattern((PVOID)codeStartVa, codeSize, CX_INITSTORAGEMEDIA_SIGNATURE, CX_INITSTORAGEMEDIA_SIGNATURE_LENGTH);
                }

                if (CxCreateStreamPtr && CxParserArchiveIndexPtr && oInitCompoundStorageMediaPtr)
                {
                    UnInlineHook(pfnGetProcAddress, HookGetProcAddress);
                    ExtractorProcess();
                }
            }
        }
    }
    return result;
}

//加载JSON配置
void LoadConfiguration() 
{
    g_enableExtract = false;
    g_packageFiles.clear();

    std::wstring jsonPath = Path::ChangeExtension(g_dllPath, L".json");
    std::string json = File::ReadAllText(jsonPath);

    cJSON* jRoot = cJSON_Parse(json.c_str());
    if (jRoot) 
    {
        //加载解包标志
        cJSON* jEnable = cJSON_GetObjectItem(jRoot, "enableExtract");
        if (jEnable)
        {
            g_enableExtract = cJSON_IsTrue(jEnable);
        }
        //启用Hash标志
        cJSON* jHashEnable = cJSON_GetObjectItem(jRoot, "enableHasher");
        if (jHashEnable) 
        {
            g_enableHasher = cJSON_IsTrue(jHashEnable);
        }

        //启用动态解码器
        cJSON* jDynamicDecoderEnable= cJSON_GetObjectItem(jRoot, "enableDynamicHashDecoder");
        if (jDynamicDecoderEnable) 
        {
            g_enableDynamicHashDecoder = cJSON_IsTrue(jDynamicDecoderEnable);
        }
        
        //延时
        cJSON* jDeleyTime = cJSON_GetObjectItem(jRoot, "extractDelayTimes");
        if (jDeleyTime) 
        {
            g_extractDelayTimes = (DWORD)cJSON_GetNumberValue(jDeleyTime);
        }

        //加载待解封包
        cJSON* jPackages = cJSON_GetObjectItem(jRoot, "packageFiles");
        if (jPackages)
        {
            if (cJSON_IsArray(jPackages))
            {
                int count = cJSON_GetArraySize(jPackages);

                for (int order = 0; order < count; ++order)
                {
                    cJSON* jItem = cJSON_GetArrayItem(jPackages, order);
                    if (jItem) 
                    {
                        char* value = cJSON_GetStringValue(jItem);

                        if (value) 
                        {
                            std::wstring packageFile = Encoding::AnsiToUnicode(value, Encoding::UTF_8);

                            if(!packageFile.empty())
                            {
                                g_packageFiles.push_back(packageFile);
                            }
                        }
                    }
                }
            }
        }
        cJSON_Delete(jRoot);
    }
}

//启用HashDumpLog文本输出
void HasherStartup() 
{
    if (g_enableHasher) 
    {
        FullCreateDirectoryW(g_hashExtractDirPath);

        std::wstring dirHashDump = g_hashExtractDirPath + L"\\_dumpDirectoryHash.log";
        std::wstring fileHashDump = g_hashExtractDirPath + L"\\_dumpFileHash.log";

        File::Delete(dirHashDump);
        File::Delete(fileHashDump);

        g_PathHashDumpLog.Open(dirHashDump.c_str());
        g_FileHashDumpLog.Open(fileHashDump.c_str());
    }
}

void OnStartup(HMODULE hModule)
{
    InlineHook(pfnGetProcAddress, HookGetProcAddress);

    g_currentDirPath.clear();
    g_extractDirPath.clear();
    g_hashExtractDirPath.clear();
    g_dllPath.clear();

    std::wstring currentDir = Path::GetDirectoryName(Util::GetModulePathW(GetModuleHandleW(NULL)));
    
    g_extractDirPath = currentDir + L"\\Archive_Output";
    g_hashExtractDirPath = currentDir + L"\\Hash_Output";
    g_currentDirPath = std::move(currentDir);

    g_dllPath = std::move(Util::GetModulePathW(hModule));

    try 
    {
        LoadConfiguration();
        OutputDebugStringW(L"Load Configuration Success");
    }
    catch(const std::exception&)
    {
        OutputDebugStringW(L"Load Configuration Failed");
    }
    HasherStartup();
}

void OnShutdown()
{
    if(g_enableHasher)
    {
        //关闭HashDump文本输出
        g_PathHashDumpLog.Close();
        g_FileHashDumpLog.Close();
    }
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    UNREFERENCED_PARAMETER(lpReserved);
    switch (ul_reason_for_call)
    {
        case DLL_PROCESS_ATTACH: 
        {
            OnStartup(hModule);
            break;
        }
        case DLL_THREAD_ATTACH: { break; }
        case DLL_THREAD_DETACH: { break; }
        case DLL_PROCESS_DETACH: 
        {
            OnShutdown();
            break;
        }
    }
    return TRUE;
}

