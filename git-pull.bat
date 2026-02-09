@echo off
setlocal

REM Đặt đường dẫn project
cd /d "%~dp0"

REM Kiểm tra xem có phải git repository không
if not exist ".git" (
    echo [ERROR] Day khong phai la git repository!
    pause
    exit /b 1
)

REM Lưu thay đổi local nếu có
echo Dang kiem tra thay doi local...
git status --porcelain > nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Khong the kiem tra git status!
    pause
    exit /b 1
)

REM Pull từ remote
echo Dang pull tu remote...
git pull

if %ERRORLEVEL% equ 0 (
    echo [SUCCESS] Pull thanh cong!
) else (
    echo [ERROR] Pull that bai! Loi code: %ERRORLEVEL%
)

pause
endlocal
