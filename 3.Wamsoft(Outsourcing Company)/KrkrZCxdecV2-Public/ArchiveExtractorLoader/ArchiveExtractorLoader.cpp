#include <windows.h>
#include <detours.h>
#include "path.h"
#include "util.h"

int wmain(int argc, wchar_t* argv[])
{
	SetConsoleTitleW(L"KrkrzCxdecV2 ArchiveExtractorLoader");
	if (argc < 2)
	{
		printf("Load Failed ---> Please Drop GameExe On Loader");
		getchar();
		return 1;
	}

	std::wstring gamePath = Path::GetFullPath(argv[1]);
	std::wstring gameDirPath = Path::GetDirectoryName(gamePath);

	std::wstring commandLine;

	for (int i = 2; i < argc; i++)
	{
		std::wstring_view v(argv[i]);

		if (v.find(L' ') != std::wstring::npos)
		{
			commandLine += L'\"';
			commandLine += argv[i];
			commandLine += L'\"';
		}
		else
		{
			commandLine += argv[i];
		}

		commandLine += L' ';
	}

	std::string dllPath = Util::GetAppDirectoryA() + "\\ArchiveExtractor.dll";

	STARTUPINFO startupInfo{ 0 };
	PROCESS_INFORMATION processInfo{ 0 };

	startupInfo.cb = sizeof(startupInfo);

	if (DetourCreateProcessWithDllW(gamePath.c_str(), const_cast<std::wstring::pointer>(commandLine.c_str()),
		NULL, NULL, FALSE, 0, NULL, gameDirPath.c_str(), &startupInfo, &processInfo, dllPath.c_str(), NULL) == FALSE)
	{
		auto msg = Util::GetLastErrorMessageA();
		printf("CreateProcess Failed ---> %s\n", msg.c_str());
		getchar();
		return 1;
	}
	else 
	{
		printf("Inject Dll Success");
	}

	WaitForSingleObject(processInfo.hProcess, INFINITE);

	CloseHandle(processInfo.hThread);
	CloseHandle(processInfo.hProcess);

	return 0;
}
