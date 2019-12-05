@echo off

echo Cleaning project
rd /s bin\
rd /s obj\
dotnet clean -r win81-x64
dotnet clean -r win10-x64
echo Restoring project
dotnet restore
echo Releasing to Windows Server 2012
dotnet publish -c Release -r win81-x64
echo Releasing to Windows Server 2016
dotnet publish -c Release -r win10-x64