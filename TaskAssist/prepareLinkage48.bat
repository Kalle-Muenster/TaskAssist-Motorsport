@echo off
set _link_=%~dp0linkage
set _arch_=%~1
set _conf_=%~2
if "%_conf_%"=="ExtremDebug" (
   set _type_=Extrem
   set _conf_=Debug
) else (
   set _type_=
)

set CONSOLASTREAMS=c:\WORKSPACE\PROJECTS\GITSPACE\Consola\bin\v4.8\%_arch_%\%_conf_%
set CONTROLLERSLIB=c:\WORKSPACE\PROJECTS\GITSPACE\ControlledValues\bin\DotNetDll\%_arch_%\%_conf_%


del /f /s /q "%_link_%\*.*"
echo ######## Cleaned linkage directory
echo ######## Copy "%CONSOLASTREAMS%\*.dll" to "%_link_%"
echo ######## Copy "%CONTROLLERSLIB%\*.dll" to "%_link_%"

copy /Y /B "%CONSOLASTREAMS%\*.dll" "%_link_%"
copy /Y /B "%CONTROLLERSLIB%\*.dll" "%_link_%"
if "%_conf_%"=="Debug" (
copy /Y /B "%CONSOLASTREAMS%\*.pdb" "%_link_%"
copy /Y /B "%CONTROLLERSLIB%\*.pdb" "%_link_%"
) else (
del /s /q "%_link_%\*.pdb"
)


set _link_=
set _arch_=
set _conf_=
set _type_=
