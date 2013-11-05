@echo off

if "%EMULATED%"=="true" goto :EOF

if exist "%ProgramFiles%"\SplunkUniversalForwarder goto :EOF

cd startup

echo Getting MSIs
deployblob.exe /downloadFrom role-install /downloadTo .

echo Installing Splunk
msiexec.exe /l* splunk.log /i splunkforwarder-4.3.2-123586-x64-release.msi RECEIVING_INDEXER="%SPLUNKENDPOINT%" AGREETOLICENSE=Yes /quiet
"%ProgramFiles%"\SplunkUniversalForwarder\bin\Splunk.exe add tcp "%SPLUNKLOCALPORT%" -auth admin:changeme >splunkinit.log 2>&1

