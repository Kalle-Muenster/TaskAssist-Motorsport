@if not "ECHO_STATE"=="" (@echo %ECHO_STATE%) else (@echo off)
if "%DotNetVersionString%"=="dot48" set DotNetVersionNumber=48
if "%DotNetVersionString%"=="dot60" set DotNetVersionNumber=60
if "%DotNetVersionString%"=="core5" set DotNetVersionNumber=50
set _here_=%CD%
cd /d %~dp0
cd TaskAssist
dotnet restore TaskAssist%DotNetVersionNumber%.csproj
cd..
cd TestAssist
dotnet restore TestAssist%DotNetVersionNumber%.csproj
cd /d %_here_%
set _here_=

