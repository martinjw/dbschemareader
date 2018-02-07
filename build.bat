:: vs2015 build
:: %systemroot%\Microsoft.Net\Framework\v4.0.30319\MSBuild.exe build.proj /t:Release /fl /flp:Verbosity=normal & pause
:: dotnet pack DatabaseSchemaReader\DatabaseSchemaReader.csproj -c Release -o nuget

echo oFF
setlocal enabledelayedexpansion

:: one (or more) of these VS2017 editions may be installed
set community="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MsBuild.exe"
set pro="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MsBuild.exe"
set ent="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MsBuild.exe"

if exist %community% (
	%community% DatabaseSchemaReader.sln /t:restore
	%community% build.proj /t:Release /fl /flp:Verbosity=normal
	%community% DatabaseSchemaReader\DatabaseSchemaReader.csproj /t:Pack /p:Configuration=Release
) else (
	if exist %ent% (
		%ent% DatabaseSchemaReader.sln /t:restore
		%ent% build.proj /t:Release /fl /flp:Verbosity=normal
		%ent% DatabaseSchemaReader\DatabaseSchemaReader.csproj /t:Pack /p:Configuration=Release
	) else (
		%pro% DatabaseSchemaReader.sln /t:restore
		%pro% build.proj /t:Release /fl /flp:Verbosity=normal
		%pro% DatabaseSchemaReader\DatabaseSchemaReader.csproj /t:Pack /p:Configuration=Release
	)
)

copy DatabaseSchemaReader\bin\Release\*.nupkg nuget

pause