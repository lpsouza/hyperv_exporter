@echo off
REM Windows 2016
set RUNTIME="win10-x64"
REM Windows 2012 R2
REM set RUNTIME="win81-x64"

REM Publish
dotnet clean
dotnet publish -c Release --runtime %RUNTIME%
explorer %~dp0bin\Release\netcoreapp2.1\%RUNTIME%\publish\
