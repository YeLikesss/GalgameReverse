

//获取序号是否合法
bool OrdinalIsValid(long long ordinal);

//使用序号获取封包混淆名 (最多8字节 4个字符 3个Unicode字符 + 0结束符)
void GetFakeNameWithOrdinal(long long ordinal,wchar_t* fakeNameRet);

//使用序号获取加密模式
unsigned int GetEncryptModeWithOrdinal(long long ordinal);