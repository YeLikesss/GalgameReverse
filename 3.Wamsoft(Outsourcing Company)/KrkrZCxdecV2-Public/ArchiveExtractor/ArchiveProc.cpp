#include "ArchiveProc.h"


//获取流长度
unsigned long long StreamGetLength(IStream* stream)
{
    //获取当前位置
    LARGE_INTEGER pos{ 0 };
    pos.QuadPart = StreamGetPosition(stream);

    //获取流长度
    unsigned long long size;
    stream->Seek(LARGE_INTEGER{ 0 }, STREAM_SEEK_END, (ULARGE_INTEGER*)&size);

    //恢复原来位置
    stream->Seek(pos, STREAM_SEEK_SET, NULL);
    return size;
}

//获取流当前位置
unsigned long long StreamGetPosition(IStream* stream)
{
    unsigned long long pos;
    stream->Seek(LARGE_INTEGER{ 0 }, STREAM_SEEK_CUR, (ULARGE_INTEGER*)&pos);
    return pos;
}

//设置流的位置
void StreamSeek(IStream* stream, long long offset, DWORD seekMode) 
{
    LARGE_INTEGER move{ 0 };
    move.QuadPart = offset;
    stream->Seek(move, seekMode, NULL);
}

//读取流
unsigned long StreamRead(IStream* stream, void* buffer, unsigned long length) 
{
    unsigned long readLength = 0;
    stream->Read(buffer, length, &readLength);
    return readLength;
}

//尝试解密SimpleEncrypted
bool TryDecryptText(IStream* stream, std::vector<uint8_t>& output)
{
    try
    {
        uint8_t mark[2] { 0 };
        StreamRead(stream, mark, 2);

        if (mark[0] == 0xfe && mark[1] == 0xfe)   //检查加密标记头
        {
            uint8_t mode;
            StreamRead(stream, &mode, 1);

            if (mode != 0 && mode != 1 && mode != 2)  //识别模式
            {
                return false;
            }

            ZeroMemory(mark, sizeof(mark));
            StreamRead(stream, mark, 2);

            if (mark[0] != 0xff || mark[1] != 0xfe)  //Unicode Bom
            {
                return false;
            }

            if (mode == 2)   //压缩模式
            {
                long long compressed = 0;
                long long uncompressed = 0;
                StreamRead(stream, &compressed, sizeof(long long));
                StreamRead(stream, &uncompressed, sizeof(long long));

                if (compressed <= 0 || compressed >= INT_MAX || uncompressed <= 0 || uncompressed >= INT_MAX)
                {
                    return false;
                }

                std::vector<uint8_t> data((size_t)compressed);

                //读取压缩数据
                if (StreamRead(stream, data.data(), compressed) != compressed)
                {
                    return false;
                }

                std::vector<uint8_t> buffer((unsigned int)uncompressed + 2);

                //写入Bom头
                buffer[0] = mark[0];
                buffer[1] = mark[1];

                Bytef* dest = buffer.data() + 2;
                uLongf destLen = (uLongf)uncompressed;

                int result = Z_OK;

                try
                {
                    result = uncompress(dest, &destLen, data.data(), (uLong)compressed);  //Zlib解压
                }
                catch (...)
                {
                    return false;
                }

                if (result != Z_OK || destLen != (uLongf)uncompressed)
                {
                    return false;
                }

                output = std::move(buffer);

                return true;
            }
            else
            {
                long long startpos = StreamGetPosition(stream); //解密起始位置
                long long endpos = StreamGetLength(stream); //解密结束位置

                StreamSeek(stream, startpos, STREAM_SEEK_SET);      //设置回起始位置

                long long size = endpos - startpos;   //解密大小

                if (size <= 0 || size >= INT_MAX)
                {
                    return false;
                }

                size_t count = (size_t)(size / sizeof(wchar_t));

                if (count == 0)
                {
                    return false;
                }

                std::vector<wchar_t> buffer(count);  //存放文本

                StreamRead(stream, buffer.data(), size);  //读取资源

                if (mode == 0)  //模式0
                {
                    for (size_t i = 0; i < count; i++)
                    {
                        wchar_t ch = buffer[i];
                        if (ch >= 0x20) buffer[i] = ch ^ (((ch & 0xfe) << 8) ^ 1);
                    }
                }
                else if (mode == 1)   //模式1
                {
                    for (size_t i = 0; i < count; i++)
                    {
                        wchar_t ch = buffer[i];
                        ch = ((ch & 0xaaaaaaaa) >> 1) | ((ch & 0x55555555) << 1);
                        buffer[i] = ch;
                    }
                }

                size_t sizeToCopy = count * sizeof(wchar_t);

                output.resize(sizeToCopy + 2);

                //写入Unicode Bom
                output[0] = mark[0];
                output[1] = mark[1];

                //回写解密后的数据
                memcpy(output.data() + 2, buffer.data(), sizeToCopy);

                return true;
            }
        }
    }
    catch (...)
    {
    }

    return false;
}




