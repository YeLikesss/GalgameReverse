#include "IndexProc.h"


//获取序号是否合法
bool OrdinalIsValid(long long ordinal)
{
	return ordinal >= 0;
}

//使用序号获取封包混淆名 (最多8字节 4个字符 3个Unicode字符 + 0结束符)
void GetFakeNameWithOrdinal(long long ordinal,wchar_t* fakeNameRet) 
{
    wchar_t* fakeName = fakeNameRet;

    *(long long*)fakeName = 0;      //清空8字节

	unsigned long ordinalLow32 = ordinal & 0x00000000FFFFFFFF;

	unsigned long temp = ordinalLow32;
	int charIndex = 0;
    do
    {
        temp &= 0x00003FFF;
        temp += 0x00005000;

        fakeName[charIndex] = temp & 0x0000FFFF;
        ++charIndex;

        ordinalLow32 >>= 0x0E;
    } while (ordinalLow32 != 0);
}

//使用序号获取加密模式
unsigned int GetEncryptModeWithOrdinal(long long ordinal)
{
    return ((ordinal & 0x0000FFFF00000000) >> 32);
}