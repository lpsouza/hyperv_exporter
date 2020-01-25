@echo off

cls
echo # Cleaning project
rd /s %~dp0libs\prometheus\bin\
rd /s %~dp0libs\prometheus\obj\
rd /s %~dp0src\bin\
rd /s %~dp0src\obj\
rd /s %~dp0publish\
mkdir %~dp0publish
dotnet clean -r win81-x64
dotnet clean -r win10-x64
echo # Restoring project
dotnet restore
echo # Releasing to Windows Server 2012
dotnet publish -c Release -r win81-x64
move %~dp0src\bin\Release\netcoreapp3.0\win81-x64\publish %~dp0publish\win81-x64
del %~dp0publish\win81-x64\hyperv_exporter.pdb
del %~dp0publish\win81-x64\web.config
echo # Releasing to Windows Server 2016
dotnet publish -c Release -r win10-x64
move %~dp0src\bin\Release\netcoreapp3.0\win10-x64\publish %~dp0publish\win10-x64
del %~dp0publish\win10-x64\hyperv_exporter.pdb
del %~dp0publish\win10-x64\web.config
