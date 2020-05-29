@echo off
set extra=%*
echo Restoring...
msbuild -noLogo -m -clp:ErrorsOnly -t:restore
if errorlevel 1 exit /b %errorlevel%
echo Build one...
msbuild -noLogo -m -clp:ErrorsOnly %extra% 
if errorlevel 1 exit /b %errorlevel%
set f=%tmp%\ibg.flag
del %f% > nul
echo Build two...
msbuild -noLogo %extra% "-logger:IncrementalBuildGuardLogger,%~dp0IncrementalBuildGuard.dll;%f%" -clp:PerformanceSummary;v=q
if errorlevel 1 exit /b %errorlevel%
set issue=
if exist "%f%" set issue=1
if not "%issue%"=="" echo "ERROR: Compiler triggered during incremental build"
if not "%issue%"=="" exit /b 1