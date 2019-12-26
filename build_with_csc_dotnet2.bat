set CSC=
set OPT=

REM 32bit

if EXIST C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe (
    set CSC=C:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe
    set OPT=
)

REM 64bit

if EXIST C:\Windows\Microsoft.NET\Framework64\v2.0.50727\csc.exe (
    set CSC=C:\Windows\Microsoft.NET\Framework64\v2.0.50727\csc.exe
    set OPT=
)

%CSC% %OPT% /target:winexe /r:Microsoft.VisualBasic.dll /win32icon:ImageViewer\icon\app.ico /out:ImageViewer.exe ImageViewer\*.cs
pause
