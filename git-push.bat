@echo off
chcp 65001 >nul
title Git Auto Push
echo ====================================
echo   Git Auto Push
echo ====================================
echo.
set /p message="Nhập commit message: "

if "%message%"=="" (
    echo Error: Message không được để trống!
    pause
    exit /b 1
)

echo.
echo Đang push với message: "%message%"
echo.
"C:\Program Files\Git\bin\bash.exe" -c "cd '%cd%' && ./git-auto.sh -p '%message%'"
echo.
pause
