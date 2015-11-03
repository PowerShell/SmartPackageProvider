# SMART & EXE Package Providers
Microsoft Hackathon 2015 project - find, download and install applications in one line of PowerShell. Supports MSI and EXE installation.

Note that this is an experimental project and is not officially supported by Microsoft. Use at your own risk - only install applications from sources you trust. Contributions are very welcome!

# Usage
```powershell
find-package -provider smart notepad++ | select -first 1 | install-package
```

# Deploy SMART Installer on your system
1. Build SMART and EXE solutions in Visual Studio.
2. Copy providers and dependent dlls to PackageManagement folder. The folder should be either one of:
```powershell
C:\Program Files\WindowsPowerShell\Modules\PackageManagement\1.0.0.0\ 
```
or
```powershell
C:\Program Files\WindowsPowerShell\Modules\PackageManagement
```
You can run
```powershell
Get-Module PackageManagement -list
```
to get the actual location.

The files to copy are:

 - SmartProvider.dll
 - ExeProvider.dll
 - Newtonsoft.Json.dll
 - HtmlAgilityPack.dll

# Check Installation
```powershell
Get-PackageProvider
```
You should see SMART and EXE providers.

# Verified with:
 - Notepad++
 - Vivaldi
 - 7zip
 - Icon Restore