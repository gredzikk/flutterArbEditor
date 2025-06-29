@echo off
echo Registering .aep file association...

rem Get the current executable path
set "exePath=%~dp0flutterArbEditor.exe"

rem Register the file extension
reg add "HKEY_CURRENT_USER\Software\Classes\.aep" /ve /d "ARBEditorProject" /f
reg add "HKEY_CURRENT_USER\Software\Classes\ARBEditorProject" /ve /d "ARB Editor Project" /f
reg add "HKEY_CURRENT_USER\Software\Classes\ARBEditorProject\DefaultIcon" /ve /d "\"%exePath%\",0" /f
reg add "HKEY_CURRENT_USER\Software\Classes\ARBEditorProject\shell\open\command" /ve /d "\"%exePath%\" \"%%1\"" /f

echo File association registered successfully!
pause