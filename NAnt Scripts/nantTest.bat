echo Launching NAnt with %1
REM :: Use NAnt to build project
@tools\nant\NAnt.exe -buildfile:%1
