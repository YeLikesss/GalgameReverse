#include "KrPath.h"

//斜杠转反斜杠
void FixWindowsPath(std::wstring& path)
{
	for (size_t i = 0; i < path.length(); i++)
	{
		if (path[i] == L'/')
		{
			path[i] = L'\\';
		}
	}
}


//反斜杠转斜杠  大写转小写
void FixXP3Path(std::wstring& path)
{
	for (size_t i = 0; i < path.length(); i++)
	{
		if (path[i] == L'\\')
		{
			path[i] = L'/';
		}

		if (path[i] >= L'A' && path[i] <= L'Z')
		{
			path[i] |= 0x20;
		}
	}
}

//获取资源相对路径
const wchar_t* GetRelativePath(const wchar_t* path)
{
	int length = 0;

	const wchar_t* newPath = path;

	//获取字符串长度 字符串移到最后
	while (*newPath != '\0')
	{
		length++;
		newPath++;
	}

	//小于两个字符
	if (length < 2)
	{
		return newPath;
	}

	//扫描封包分割符号
	while (length != 0)
	{
		if (*newPath == '>')
		{
			return newPath + 1;
		}
		newPath--;
		length--;
	}

	return newPath;
}


//转换路径到封包标准格式
void NormalizeXP3VFSPath(std::wstring& path)
{
	FixXP3Path(path);
}

//将文件夹路径转化封包格式
std::wstring GetXP3VFSCurrentDirectoryPath(const std::wstring& currentDirPath)
{
	wchar_t diskVol[2]{ 0 };
	diskVol[0] = currentDirPath.c_str()[0];       //获取盘符

	std::wstring xp3CurrentDir(L"file://./");
	xp3CurrentDir += diskVol;         //添加盘符
	xp3CurrentDir += &currentDirPath.c_str()[2];        //添加文件夹路径

	return xp3CurrentDir;
}

//获取标准格式封包路径
std::wstring NormalizeXP3PackagePath(const std::wstring& xp3Dir,const std::wstring& packageFileName)
{
	std::wstring xp3Path = xp3Dir;
	xp3Path += L"\\";
	xp3Path += packageFileName;

	NormalizeXP3VFSPath(xp3Path);
	return xp3Path;
}