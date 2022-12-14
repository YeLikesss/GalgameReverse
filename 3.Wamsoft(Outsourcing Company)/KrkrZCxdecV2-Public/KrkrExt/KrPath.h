#include <string>

//斜杠转反斜杠
void FixWindowsPath(std::wstring& path);

//反斜杠转斜杠
void FixXP3Path(std::wstring& path);

//获取资源相对路径
const wchar_t* GetRelativePath(const wchar_t* path);

//转换路径到封包标准格式
void NormalizeXP3VFSPath(std::wstring&);

//获取文件夹路径
std::wstring GetXP3VFSCurrentDirectoryPath(const std::wstring& currentDirPath);

//获取标准格式封包路径
std::wstring NormalizeXP3PackagePath(const std::wstring& xp3Dir, const std::wstring& packageFileName);