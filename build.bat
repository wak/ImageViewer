@echo off

set CSC=
set OPT=

REM 32bit

if EXIST C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe (
    set CSC=C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe
    set OPT=
)

if EXIST C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe (
    set CSC=C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe
    set OPT=/win32manifest:ImageViewer\app.manifest
)

if EXIST C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe (
    set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
    set OPT=/win32manifest:ImageViewer\app.manifest
)


REM 64bit

if EXIST C:\Windows\Microsoft.NET\Framework64\v2.0.50727\csc.exe (
    set CSC=C:\Windows\Microsoft.NET\Framework64\v2.0.50727\csc.exe
    set OPT=
)

if EXIST C:\Windows\Microsoft.NET\Framework64\v3.5\csc.exe (
    set CSC=C:\Windows\Microsoft.NET\Framework64\v3.5\csc.exe
    set OPT=/win32manifest:ImageViewer\app.manifest
)

if EXIST C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe (
    set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
    set OPT=/win32manifest:ImageViewer\app.manifest
)

echo ============================================================
echo = CSC: %CSC%
echo = OPT: %OPT%
echo ============================================================

%CSC% %OPT% /nologo /target:winexe /r:Microsoft.VisualBasic.dll /win32icon:ImageViewer\icon\app.ico /out:ImageViewer.exe ImageViewer\*.cs

if %ERRORLEVEL% == 0 (
  echo Build succeed.
) else (
  echo Build failed.
)

echo.

pause
