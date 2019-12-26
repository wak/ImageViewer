@echo off

set MSBUILD=
set OPT=
set SLN=ImageViewer.sln

REM 32bit

if EXIST C:\Windows\Microsoft.NET\Framework\v2.0.50727\msbuild.exe (
    set MSBUILD=C:\Windows\Microsoft.NET\Framework\v2.0.50727\msbuild.exe
)

if EXIST C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe (
    set MSBUILD=C:\Windows\Microsoft.NET\Framework\v3.5\msbuild.exe
)

if EXIST C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe (
    set MSBUILD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
)

REM 64bit

if EXIST C:\Windows\Microsoft.NET\Framework64\v2.0.50727\msbuild.exe (
    set MSBUILD=C:\Windows\Microsoft.NET\Framework64\v2.0.50727\msbuild.exe
)

if EXIST C:\Windows\Microsoft.NET\Framework64\v3.5\msbuild.exe (
    set MSBUILD=C:\Windows\Microsoft.NET\Framework64\v3.5\msbuild.exe
)

if EXIST C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe (
    set MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe
)

%MSBUILD% %OPT% /nologo /p:Configuration=Release %SLN%

echo =================================
if %ERRORLEVEL% == 0 (
    echo Build succeed.
    echo   =^> bin/Release/ImageViewer.exe
) else (
    echo Build failed.
)

echo.
pause
