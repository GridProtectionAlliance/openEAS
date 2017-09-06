@ECHO OFF
SET target=%1
SET source=%2
IF /I "%source%"=="" SET source=PQMarkPusher
IF /I "%target%"=="" SET /p target="Enter project name: "
IF /I "%target%"=="" (
    ECHO ERROR: Please enter project name.
    EXIT
)
ECHO.
ECHO *** openEAS Rename Script ***
ECHO.
ECHO About to rename "%source%" to "%target%", press Ctrl+C to cancel, or
PAUSE
.\Source\Dependencies\GSF\ReplaceInFiles /r /v /c ".\Build\*.*" %source% %target%
.\Source\Dependencies\GSF\ReplaceInFiles /r /v /c ".\Source\*.*" %source% %target%
.\Source\Dependencies\GSF\BRC64 /REPLACECI:%source%:%target% /RECURSIVE /EXECUTE
ECHO.
ECHO Project Rename Complete.
ECHO.
