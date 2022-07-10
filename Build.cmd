:: @if "%ECHO_STATE%"=="" (@echo off ) else (@echo %ECHO_STATE% )
@echo on
if "%DotNetVersionString%"=="core5" set _vers_=50
if "%DotNetVersionString%"=="dot48" set _vers_=48
if "%_vers_%"=="" goto ERROR

:: Prepare locations
set _name_=Motorsport
set _call_=%CD%
cd %~dp0
set _here_=%CD%
set _root_=%CD%

:: Set VersionNumber
set MotorsportsVersionNumber=00000001
set MotorsportsVersionString=0.0.0.1

:: Set Dependencies
if "%ConsolaBinRoot%"=="" (
set ConsolaBinRoot=.\..\..\Consola\bin\%DotNetVersionString%
)
if "%Int24TypesBinRoot%"=="" (
set Int24TypesBinRoot=.\..\..\Int24Types\bin\%DotNetVersionString%
)
if "%ControlledValuesBinRoot%"=="" (
set ControlledValuesBinRoot=.\..\..\ControlledValues\bin\%DotNetVersionString%
)

:: Set parameters and solution files
call %_root_%\Args "%~1" "%~2" "%~3" "%~4" TaskAssist%_vers_%.sln
set _vers_=

:: Do the Build
cd %_here_%
call MsBuild %_target_% %_args_%
cd %_call_%

:: Cleanup Environment
call %_root_%\Args ParameterCleanUp

goto DONE

:ERROR
echo.
echo Variable 'DotNetVersionString' must be set
echo.
:DONE


