param(
    [string]$installPath
)

If (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    $arguments = "& '" + $myinvocation.mycommand.definition + "' " + (Get-Location).Path
    Start-Process powershell -Verb runAs -ArgumentList $arguments
    Break
}

$params = @{
    Name = "hyperv_exporter"
    BinaryPathName = "$installPath\hyperv_exporter.exe"
    DisplayName = "Hyper-V Exporter for Prometheus"
    StartupType = "Automatic"
    Description = "Hyper-V Exporter for Prometheus"
}
New-Service @params
Start-Service $params.Name
