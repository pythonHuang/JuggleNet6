@echo off
chcp 65001 >nul
:: ============================================================
:: Juggle 接口编排平台 - Windows 一键启动脚本
:: Version: 1.0
:: ============================================================

echo.
echo  ╔═══════════════════════════════════════════════════════╗
echo  ║     Juggle 接口编排平台 - 一键启动 (Windows)           ║
echo  ╚═══════════════════════════════════════════════════════╝
echo.

:: 获取脚本所在目录
set SCRIPT_DIR=%~dp0
cd /d "%SCRIPT_DIR%"

:menu
echo 请选择启动方式:
echo.
echo   [1] Docker 启动 (推荐，需要 Docker Desktop)
echo   [2] 直接运行 .NET 项目 (需要 .NET 8 SDK)
echo   [3] 仅构建 Docker 镜像
echo   [4] 清理 Docker 容器和镜像
echo   [5] 查看日志 (Docker模式)
echo   [0] 退出
echo.
set /p choice=请输入选项 [1-5, 0退出]:

if "%choice%"=="1" goto docker_start
if "%choice%"=="2" goto dotnet_run
if "%choice%"=="3" goto docker_build
if "%choice%"=="4" goto docker_clean
if "%choice%"=="5" goto docker_logs
if "%choice%"=="0" goto end

echo.
echo [错误] 无效选项，请重新选择!
echo.
goto menu

:docker_start
echo.
echo [1/3] 检查 Docker 状态...
docker info >nul 2>&1
if errorlevel 1 (
    echo [错误] Docker 未运行，请先启动 Docker Desktop
    echo.
    pause
    goto menu
)
echo [OK] Docker 运行正常

echo.
echo [2/3] 检查 Docker 镜像是否存在...
docker images pythonhuang/juggle-net8:v1.0 -q >nul 2>&1
if errorlevel 1 (
    echo [提示] 本地未找到镜像，正在尝试拉取...
    docker pull pythonhuang/juggle-net8:v1.0
    if errorlevel 1 (
        echo [提示] 拉取失败，尝试本地构建...
        goto docker_build
    )
)

echo.
echo [3/3] 启动 Juggle 容器...
docker run -d ^
    --name juggle ^
    --restart unless-stopped ^
    -p 9127:9127 ^
    -v juggle_data:/data ^
    -e ASPNETCORE_ENVIRONMENT=Production ^
    -e ASPNETCORE_URLS=http://+:9127 ^
    -e DB_PATH=/data/juggle.db ^
    pythonhuang/juggle-net8:v1.0

if errorlevel 1 (
    echo.
    echo [错误] 容器启动失败，可能端口已被占用
    echo.
    goto menu
)

echo.
echo ╔══════════════════════════════════════════════════════════╗
echo ║  Juggle 启动成功!                                         ║
echo ╠══════════════════════════════════════════════════════════╣
echo ║  访问地址: http://localhost:9127                          ║
echo ║  默认账号: juggle / juggle                                ║
echo ╚══════════════════════════════════════════════════════════╝
echo.
echo 提示: 使用选项 [5] 可查看运行日志
echo.
pause
goto end

:docker_build
echo.
echo [1/2] 停止并移除旧容器 (如果存在)...
docker stop juggle >nul 2>&1
docker rm juggle >nul 2>&1

echo.
echo [2/2] 构建 Docker 镜像 (这可能需要几分钟)...
docker build -t pythonhuang/juggle-net8:v1.0 .
if errorlevel 1 (
    echo.
    echo [错误] Docker 构建失败
    echo.
    pause
    goto menu
)

echo.
echo [OK] 镜像构建完成!
echo.
set /p start_now=是否立即启动? [Y/N]:
if /i "%start_now%"=="Y" goto docker_start
goto menu

:docker_clean
echo.
echo [警告] 即将清理 Docker 资源...
echo.
docker stop juggle >nul 2>&1
docker rm juggle >nul 2>&1
docker volume rm juggle_data >nul 2>&1
echo [OK] 清理完成
echo.
pause
goto menu

:docker_logs
docker logs -f juggle
goto end

:dotnet_run
echo.
echo [1/4] 检查 .NET SDK 版本...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [错误] 未安装 .NET 8 SDK，请先安装:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    goto menu
)
for /f "delims=" %%v in ('dotnet --version') do set DOTNET_VER=%%v
echo [OK] .NET 版本: %DOTNET_VER%

echo.
echo [2/4] 还原 NuGet 包...
dotnet restore Juggle.sln
if errorlevel 1 (
    echo [错误] 包还原失败
    echo.
    pause
    goto menu
)

echo.
echo [3/4] 构建项目...
dotnet build Juggle.sln -c Release
if errorlevel 1 (
    echo [错误] 项目构建失败
    echo.
    pause
    goto menu
)

echo.
echo [4/4] 启动服务...
echo.
echo ╔══════════════════════════════════════════════════════════╗
echo ║  Juggle 启动成功!                                         ║
echo ╠══════════════════════════════════════════════════════════╣
echo ║  访问地址: http://localhost:9127                          ║
echo ║  默认账号: juggle / juggle                                ║
echo ║  按 Ctrl+C 停止服务                                       ║
echo ╚══════════════════════════════════════════════════════════╝
echo.
dotnet run --project Juggle.Api/Juggle.Api.csproj --no-build
goto end

:end
echo.
echo再见!
