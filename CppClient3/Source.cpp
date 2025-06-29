#include "iostream"
#include "tchar.h"
#include "Windows.h"
#include "winsock.h"

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lCmdLine, int nCmdShow)
{
	WNDCLASSEX wndclass;
	TCHAR szClassName[] = TEXT("MyWindow");
	TCHAR szAppName[] = TEXT("HelloWindow");
	HWND hwnd;
	MSG msg;

	wndclass.cbSize = sizeof(WNDCLASSEX);
	wndclass.style = CS_HREDRAW | CS_VREDRAW | CS_DBLCLKS;
	wndclass.lpfnWndProc = WindowProc;
	wndclass.cbClsExtra = 0;
	wndclass.cbWndExtra = 0;
	wndclass.hInstance = hInstance;
	wndclass.hIcon = LoadIcon(NULL, IDI_APPLICATION);
	wndclass.hCursor = LoadCursor(NULL, IDC_ARROW);
	wndclass.hbrBackground = (HBRUSH)GetStockObject(WHITE_BRUSH);
	wndclass.lpszMenuName = NULL;
	wndclass.lpszClassName = szClassName;

	wndclass.hIconSm = NULL;
	RegisterClassEx(&wndclass);

	hwnd = CreateWindowEx(WS_EX_WINDOWEDGE, szClassName, szAppName, WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, CW_USEDEFAULT, 300, 180, NULL, NULL, hInstance, NULL);
	ShowWindow(hwnd, nCmdShow);
	UpdateWindow(hwnd);

	//HWND hwnd2 = CreateWindowEx(0, szClassName, szAppName, WS_OVERLAPPEDWINDOW, 300, 200, 300, 180, NULL, NULL, hInstance, NULL);
	//ShowWindow(hwnd2, nCmdShow);
	//UpdateWindow(hwnd2);

	while (GetMessage(&msg, NULL, 0, 0) != 0)
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return msg.wParam;
}

LRESULT CALLBACK WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	switch (uMsg)
	{
	case WM_CREATE:
		return 0;
	case WM_DESTROY:
		PostQuitMessage(0);
		return 0;
	case WM_CLOSE:
		int nClose = MessageBox(hwnd, TEXT("Are you sure to close?"), TEXT("WinForm Close"), MB_YESNO);
		if (nClose == IDYES)
			DestroyWindow(hwnd);

		return 0;
	}

	return DefWindowProc(hwnd, uMsg, wParam, lParam);
}