set PATH=%PATH%;C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin

rem go to current folder
cd %~dp0

msbuild write_VersionInfo.xml
msbuild build_output.xml /property:OutputTemp=..\OutputTemp /property:BuildDir=..\Output /property:Lib=..\Lib /property:Help=..\FreePIE.Core.Plugins\Help
msbuild build_installer.xml /property:InstallerTemp=..\InstallerTemp /property:Lib=..\Lib
pause