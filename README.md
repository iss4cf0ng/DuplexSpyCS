# DuplexSpy
DuplexSpyCS (CS stands for CSharp) is a open source RAT base on C/S

Continuously update: [Branch-Version2.0.0](https://github.com/iss4cf0ng/DuplexSpyCS/tree/version-2.0.0)

## Documents

The online documents are here:
[DuplexSpy](https://iss4cf0ng.github.io/2026/01/28/2026-1-28-ToolsDuplexSpy-v2-0-0/)
[Fileless Execution](https://iss4cf0ng.github.io/2026/02/03/2026-2-3-DuplexSpyFilelessExec/)
[DLL and shellcode injector and loader](https://iss4cf0ng.github.io/2026/02/03/2026-2-3-DuplexSpyDllAndShellcode/)
[Remote Plugin](https://iss4cf0ng.github.io/2026/02/03/2026-2-3-DuplexSpyPlugin/)
[Proxy](https://iss4cf0ng.github.io/2026/02/03/2026-2-3-DuplexSpyProxy/)

# Introduction
DuplexSpy incorporates features inspired by other tools as well as my own personal experience.  
Compared to the previous version, I removed several features that I considered unnecessary and added a number of new ones.  
Throughout this development process, I learned a great deal, and I sincerely hope that this project can be useful to others who are interested in offensive security or malware research.

If you encounter any issues or have suggestions, please feel free to open an issue on the repository page.

# Disclaimer
This project was developed as part of my personal interest in studying cybersecurity. However, it may potentially be misused for malicious purposes.  
Please do NOT use this tool for any illegal activities.

<p align="center">
  <img src="https://iss4cf0ng.github.io/images/meme/mika_punch.jpg" width="500">
</p>

# Murmur
As a college student, developing a GUI-based remote access tool entirely on my own—and performing proper quality assurance (QA)—has been a significant challenge for me.  
Due to limited time, experience, and resources, this project may still contain defects or design flaws that I have not yet discovered.  
Nevertheless, I believe that I have successfully built a RAT that incorporates a variety of offensive techniques and practical features.

If you find this project helpful or informative, I would truly appreciate a ⭐ on the repository. Your support would be a great motivation for me to continue improving this tool.

<p align="center">
  <img src="https://iss4cf0ng.github.io/images/meme/hakari_crying.jpeg" width="600">
</p>

# Acknowledgement
To all cybersecurity experts, researchers and remote access tool authors, respect!

# Feature
- Client Config
- Information
- Manager
  - File Manager
  - Task Manager
    - DLL Injection
    - Shellcode Injection
  - Registy Editor
  - Service Manager
  - Connection View
  - Window Manager
- Terminal
  - Piped Shell
  - Xterm Shell [See this](https://iss4cf0ng.github.io/2026/01/28/2026-1-28-ToolsDuplexSpy-v2-0-0/#Xterm-Terminal)
  - WQL Console
- Remote Desktop
- Camera View
- Audio
  - Capturing Microphone Signal
  - Capturing Speaker Signal
  - Play Sounds
- FunStuff
  - Hide/Show Mouse
  - Lock/Unlock Mouse
  - Hide/Show Tray
  - Hide/Show Taskbar
  - Hide/Show Clock
  - Hide/Show StartOrb
  - Change Wallpaper
  - Lock/Unlock Screen
- Proxy ([click here to learn more about this feature](https://iss4cf0ng.github.io/2026/02/03/2026-2-3-DuplexSpyProxy/))
- Plugins ([click here to learn more about this feature](https://iss4cf0ng.github.io/2026/02/03/2026-2-3-DuplexSpyPlugin/))
- Mics
  - DLL Loader
  - Shellcode Loader
  - Fieless Execution ([click here to learn more about this feature](https://iss4cf0ng.github.io/2026/02/03/2026-2-3-DuplexSpyFilelessExec/))
  - Run Script
  - Chat Message
  - KeyLogger
- Batch
  - Multi Desktop
  - Multi Webcam
  - Open URL
  - Lock Screen ([See this](https://iss4cf0ng.github.io/2026/01/28/2026-1-28-ToolsDuplexSpy-v2-0-0/#Lock-Screen))

# Anticipation (Coming soon, in v3.0.0)
- Quality Assurance (Debug)
- More Plugins
- DNS Tunneling

# Network Architecture
<p align="center">
  <img src="https://github.com/iss4cf0ng/DuplexSpyCS/blob/main/png/architecture.png" width="700">
</p>

# Screenshot
## Main
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/main/2.png" width="700">
</p>

## File Manager
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/manager/file.1.png" width="700">
</p>

## Display Image File
This RAT is able to view image file.
<p align="center">
  <img src="https://github.com/iss4cf0ng/DuplexSpyCS/blob/main/png/showimage.png" width="700">
</p>

## Task Manager
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/manager/task.1.png" width="700">
</p>

## Reg Edit
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/manager/reg.1.png" width="700">
</p>

## Service Manager
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/manager/serv.1.png" width="700">
</p>

## Connection View
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/manager/conn.1.png" width="700">
</p>

## Window Manager
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/manager/window.1.png" width="700">
</p>

<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/manager/window.2.png" width="700">
</p>

## Monitor
<p align="center">
  <img src="https://github.com/iss4cf0ng/DuplexSpyCS/blob/main/png/monitor.png" width="700">
</p>

## Shell
### Piped Shell
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/terminal/shell.png" width="700">
</p>

### Xterm Shell
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/terminal/xterm.png" width="700">
</p>

### WQL Console
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/terminal/wql.png" width="700">
</p>

## Keylogger
<p align="center">
  <img src="https://github.com/iss4cf0ng/DuplexSpyCS/blob/main/png/keylogger.png" width="700">
</p>

## Multi Desktop
<p align="center">
  <img src="https://github.com/iss4cf0ng/DuplexSpyCS/blob/main/png/multidesktop.png" width="700">
</p>

## Proxy
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/proxy/10.png" width="700">
</p>

## LockScreen
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/lockscreen/3.png" width="700">
</p>

## RunScript
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/runscript/1.png" width="700">
</p>

## Fileless Execution
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/fileless/check.2.png" width="700">
</p>

## Plugins
<p align="center">
  <img src="https://iss4cf0ng.github.io/images/article/tools/duplexspy/plugin/2.png" width="700">
</p>
