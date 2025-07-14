@echo off
setlocal

set FXC="C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\fxc.exe"

echo Compiling all .hlsl files in %CD%
echo.

for %%F in (*.hlsl) do (
    echo Compiling %%F...
    %FXC% /T cs_5_0 /E CSMain /Fo %%~nF.cso %%F
    if ERRORLEVEL 1 (
        echo.
        echo [ERROR] Failed to compile %%F
        pause
        exit /b 1
    )
)

echo.
echo All shaders compiled successfully.
pause