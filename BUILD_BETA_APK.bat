@echo off
REM ============================================================
REM  MaliGo - Build Beta APK (headless)
REM  UNITY MUST BE CLOSED before running this, or it will fail
REM  with "another Unity instance is running with this project".
REM ============================================================

set UNITY="C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe"
set PROJECT="C:\Temp\MaliGoGameDev"
set LOG="C:\Temp\MaliGoGameDev\build.log"

echo.
echo === MaliGo Beta APK build starting ===
echo First run switches platform to Android and reimports assets.
echo This can take 15-40 minutes. Do not close this window.
echo Live log: %LOG%
echo.

%UNITY% -batchmode -quit -projectPath %PROJECT% -buildTarget Android -executeMethod MaliGoBuildPipeline.BuildAndroidBeta -logFile %LOG%

echo.
if exist "C:\Temp\MaliGoGameDev\Builds\Android\MaliGo-Beta.apk" (
  echo === BUILD SUCCEEDED ===
  echo APK: C:\Temp\MaliGoGameDev\Builds\Android\MaliGo-Beta.apk
) else (
  echo === BUILD FAILED ===
  echo Open build.log and search for "error CS" or "BUILD FAILED".
)
echo.
pause
