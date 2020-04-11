:: uses vswhere, released with VS2017 update 2. For older VS, download the binary- https://github.com/Microsoft/vswhere/wiki/Installing

echo oFF
setlocal enabledelayedexpansion

for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
  set InstallDir=%%i
)
"%InstallDir%" DatabaseSchemaReader.sln /t:restore
"%InstallDir%" build.proj /t:Release /fl /flp:Verbosity=normal
"%InstallDir%" DatabaseSchemaReader\DatabaseSchemaReader.csproj /t:Pack /p:Configuration=Release

copy DatabaseSchemaReader\bin\Release\*.nupkg nuget

pause