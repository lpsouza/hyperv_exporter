@echo off
dotnet clean
dotnet publish -c Release --runtime win10-x64
explorer %~dp0bin\Release\netcoreapp2.1\win10-x64\publish\
