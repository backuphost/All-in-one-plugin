@echo off
:: Run once after clone to wire up the SDK reference DLLs.
:: Expects the PureRAT_SDK folder to be at ..\PureRAT_SDK relative to this solution.

set SDK=..\PureRAT_SDK\ReferencesDLL
set OUT=References

if not exist "%OUT%" mkdir "%OUT%"

copy /Y "%SDK%\PluginSDK.dll"    "%OUT%\PluginSDK.dll"
copy /Y "%SDK%\protobuf-net.dll" "%OUT%\protobuf-net.dll"

echo References copied.
pause
