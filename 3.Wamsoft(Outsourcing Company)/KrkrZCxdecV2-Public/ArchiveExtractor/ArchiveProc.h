#include "zlib.h"
#include <Windows.h>
#include <vector>


struct ArchiveListBinary
{
    BYTE DirectoryPathHash[8];      //文件夹Hash
    BYTE FileNameHash[32];      //文件名Hash
    BYTE FakeName[8];       //文件混淆名
    long long Key;      //资源Key
    unsigned int EncryptMode;   //加密模式
};



//获取流的长度
unsigned long long StreamGetLength(IStream* stream);
//获取流的当前位置
unsigned long long StreamGetPosition(IStream* stream);

//设置流的位置
void StreamSeek(IStream* stream, long long offset, DWORD seekMode);

//读取流
unsigned long StreamRead(IStream* stream, void* buffer, unsigned long length);

//尝试解密SimpleEncrypted
bool TryDecryptText(IStream* stream, std::vector<uint8_t>& output);
