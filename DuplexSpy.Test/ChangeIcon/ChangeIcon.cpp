#include <stdio.h>
#include <tchar.h>
#include <stdlib.h>
#include <Windows.h>

BOOL ChangeIconResource(LPCTSTR lpExeFileName, LPCTSTR lpIconFileName)
{
	HICON hIcon = (HICON)LoadImage(
		NULL, 
		lpIconFileName, 
		IMAGE_ICON, 
		0, 
		0, 
		LR_LOADFROMFILE
	);
	if (INVALID_HANDLE_VALUE == hIcon)
	{
		_tprintf(_T("Failed to load icon: %s\n"), lpExeFileName);
		return FALSE;
	}

	HANDLE hFile = CreateFile(
		lpExeFileName,
		GENERIC_READ | GENERIC_WRITE,
		0,
		NULL,
		OPEN_EXISTING,
		FILE_ATTRIBUTE_NORMAL,
		NULL
	);
	if (INVALID_HANDLE_VALUE == hFile)
	{
		_tprintf(_T("CreateFile() error, file: %s\n"), lpExeFileName);
		return FALSE;
	}

	DWORD dwSize = GetFileSize(hFile, NULL);
	PCHAR pFileData = (PCHAR)HeapAlloc(NULL, 0, dwSize);
}

int _tmain(int argc, TCHAR* argv[])
{
	LPCTSTR lpFileName = argv[1];
	LPCTSTR lpIconPath = argv[2];


}