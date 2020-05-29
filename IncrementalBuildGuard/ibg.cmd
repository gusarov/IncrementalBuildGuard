@echo off
set extra=%*
msbuild -t:restore
if errorlevel 1 exit /b %errorlevel%
msbuild -m %extra%
if errorlevel 1 exit /b %errorlevel%
set f=%tmp%\ibg.flag
del %f%
msbuild %extra% "-logger:IncrementalBuildGuardLogger,%~dp0IncrementalBuildGuard.dll;%f%" -clp:PerformanceSummary
if errorlevel 1 exit /b %errorlevel%
set issue=
if exist "%f%" set issue=1
if not "%issue%"=="" echo "ERROR: Compiler triggered during incremental build"
if not "%issue%"=="" exit /b 1