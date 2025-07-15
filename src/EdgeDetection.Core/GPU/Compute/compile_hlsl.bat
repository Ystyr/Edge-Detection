@echo off
setlocal

set FXC="C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\fxc.exe"

echo Compiling all .hlsl files in %CD%
echo.

set FAILED=0

for %%F in (*.hlsl) do (
    echo Compiling %%F...
    %FXC% /T cs_5_0 /E CSMain /Fo %%~nF.cso %%F || (
        echo [ERROR] Failed to compile %%F
        set FAILED=1
    )
)

echo.

if %FAILED%==1 (
    echo Some shaders failed to compile.
    pause
    exit /b 1
)

echo All shaders compiled successfully.
pause